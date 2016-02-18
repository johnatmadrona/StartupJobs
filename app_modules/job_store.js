var _q = require('q');
var _hash = require('object-hash');
var _s3 = require('./s3');

// This should really use a separate service like memcached or redis, but 
// given the current simplicity of this app, that's unecessary
var _cache;


///
/// Design:
/// This is a simple system for persistent storage of job descriptions. 
/// It keeps an in-process, in-memory cache of the data. This cache does 
/// not maintain consistency with the backing store, meaning that if updates 
/// are made out-of-process, the cache will be out-of-sync.
///


///
/// Utility functions
/// 

function create_key_prefix_from_company(company) {
	var normalized_company = company.toLowerCase().replace(/[^a-z^0-9^\.^-]+/g, '');
	return 'data/' + normalized_company + '/';
}

function create_key_from_job(job) {
	var key = create_key_prefix_from_company(job.company) + 'jd-' + _hash(job) + '.json';
	return key;
}

function is_job_key(str) {
	return /jd-\w+\.json$/.test(str);
}


///
/// Storage operations
///

function init(log, aws_key_id, aws_key, aws_region, s3_bucket) {
	log.debug(
		{
			aws_key_id: aws_key_id ? '(set)' : aws_key_id,
			aws_key: aws_key ? '(set)' : aws_key,
			aws_region: aws_region ? '(set)' : aws_region,
			s3_bucket: s3_bucket
		},
		'Initializing jd store'
	);

	if (typeof(_cache) !== 'undefined') {
		log.warn('Tried to initialize multiple times');
		return _q();
	}

	return _s3.init(log, aws_key_id, aws_key, aws_region, s3_bucket).then(function() {
		return get_all_jobs_from_s3(log, _s3);
	}).then(function(jobs) {
		_cache = jobs;
		log.info({ jd_count: Object.keys(jobs).length }, 'Loaded jds into cache');
	}).then(function() {
		// Remove any jobs with invalid keys. This can happen if s3 objects 
		// are manually created or there's a change in the key gen scheme.
		log.info('Checking for invalid jd keys and removing any from cache');
		var keys_to_remove = [];
		for (var jobKey in _cache) {
			if (jobKey !== create_key_from_job(_cache[jobKey])) {
				keys_to_remove.push(jobKey);
				delete(_cache[jobKey]);
			}
		}

		if (keys_to_remove.length > 0) {
			log.warn({ options }, 'Deleting invalid jds from s3');
			return _s3.remove(log, keys_to_remove).then(function() {
				log.debug(options, 'Deleted invalid jds from s3');
			});
		}
	});
}

function get_all_jobs_from_s3(log, s3, prefix, max_count) {
	log.debug({ prefix: prefix, max_count: max_count }, 'Retrieving jd list from s3');
	return s3.list(log, prefix, max_count).then(function(items) {
		return items.filter(function(item) {
			return is_job_key(item.Key);
		});
	}).then(function(items) {
		return _q.all(items.map(function(item) {
			return s3.retrieve(log, item.Key);
		}));
	}).then(function(jobs) {
		log.debug({ count: jobs.length }, 'Successfully retrieved all jds');
		var job_map = {};
		for (var i = 0; i < jobs.length; i++) {
			job_map[create_key_from_job(jobs[i])] = jobs[i];
		}
		return job_map;
	});
}

function add_jobs_to_s3(log, s3, jobs) {
	var promises = [];
	for (var i = 0; i < jobs.length; i++) {
		promises.push(add_job_to_s3(log, s3, jobs[i]));
	}
	return _q.all(promises);
}

function add_job_to_s3(log, s3, job) {
	var key = create_key_from_job(job);

	// Check if item already exists
	if (typeof(_cache[key]) !== 'undefined') {
		log.debug({ bucket: bucket, key: key }, 'Would add jd to s3, but it already exists');
		return _q();
	}

	log.debug({ key: key }, 'Adding jd to s3');
	return s3.upsert(log, key, job).then(function(result) {
		_cache[key] = job;
		log.debug({ key: key }, 'Successfully added jd to s3 and cache');
	});
}

function remove_jobs_from_s3(log, s3, jobs) {
	log.debug({ count: jobs.length }, 'Removing jds from cache');
	var keys_to_remove = [];
	for (var i = 0; i < jobs.length; i++) {
		var key = create_key_from_job(jobs[i]);
		delete(_cache[key]);
		keys_to_remove.push(key);
	}

	log.debug({ count: keys_to_remove.length }, 'Removing jds from s3');
	return s3.remove(log, keys_to_remove).then(function() {
		log.debug({ count: keys_to_remove.length }, 'Removed jds from s3');
	});
}

function query(log, query) {
	log.info({ query: query }, 'Querying jds');

	if (typeof(query.operation) !== 'string') {
		return _q.reject(new Error('Must supply string value for query.operation'));
	}

	if (query.operation === 'list-companies') {
		var company_map = Object.keys(_cache).reduce(function(cm, cache_key) {
			var company = _cache[cache_key].company;
			if (!cm[company]) {
				cm[company] = { name: company };
			}
			return cm;
		}, {});
		return _q(Object.keys(company_map).map(function(key) {
			return company_map[key];
		}));
	} else if (query.operation === 'jobs-by-company') {
		if (typeof(query.company) === 'undefined') {
			return _q.reject(new Error('Must supply value for query.company'));
		}
		var prefix = create_key_prefix_from_company(query.company);
		var jobs_by_company = Object.keys(_cache).filter(function(item) {
			return item.startsWith(prefix);
		}).map(function(cache_key) {
			return _cache[cache_key];
		});
		return _q(jobs_by_company);
	}

	return _q.reject(new Error('Invalid value for query.operation: ' + query.operation));
}

module.exports = {
	init: init,
	add_jobs: function(log, jobs) { return add_jobs_to_s3(log, _s3, jobs); },
	remove_jobs: function(log, jobs) { return remove_jobs_from_s3(log, _s3, jobs); },
	query: query
};
