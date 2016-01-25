var _q = require('q');
var _aws = require('aws-sdk');
var _hash = require('object-hash');

// This should really use a separate service like memcached or redis, but 
// given the current simplicity of this app, that's unecessary
var _config;
var _cache;

var _s3_bucket = 'madrona-sjp-test';
var _s3_prefix = 'data/';

function getS3Connection(aws) {
	return _q().then(function() { return new aws.S3(); });
}

function s3BucketExists(log, s3, bucket) {
	var d = _q.defer();

	s3.headBucket({ Bucket: bucket }, function(err, data) {
		if (err) {
			if (err.name === 'NoSuchBucket' || err.name === 'NotFound') {
				d.resolve(false);
			} else {
				d.reject(err);
			}
		} else {
			d.resolve(true);
		}
	});

	return d.promise;
}

function createS3Bucket(log, s3, bucket) {
	var options = {
		Bucket: bucket,
		ACL: 'public-read',
		CreateBucketConfiguration: {
			LocationConstraint: _config.region
		}
	};

	log.info('Creating s3 bucket: ' + bucket);
	return _q.ninvoke(s3, 'createBucket', options).then(function(result) {
		log.info('Created s3 bucket: ' + bucket)
		return true;
	});
}

function getAllJobsFromS3(log, s3, bucket, prefix, max_count) {
	var options = {
		Bucket: bucket
	};

	if (typeof(prefix) !== 'undefined' && prefix !== null) {
		options.Prefix = prefix;
	}

	if (typeof(max_count) !== 'undefined') {
		options.MaxKeys = max_count;
	}

	log.info({ bucket: bucket, prefix: prefix }, 'Retrieving job list from s3');
	return _q.ninvoke(s3, 'listObjects', options).then(function(result) {
		log.info({ bucket: bucket, prefix: prefix, count: result.Contents.length }, 'Retrieved job list from s3' );

		if ((typeof(max_count) === 'undefined' || result.Contents.length < max_count) && result.IsTruncated) {
			//var nextMarker = result.Contents[data.Contents.length - 1].Key;
			throw new Error('Truncation handling not yet implemented');
		}

		return _q.all(
			result.Contents.filter(function(item) {
				return /\.jd$/.test(item.Key);
			}).map(function(item) {
				return getJobFromS3(log, s3, bucket, item.Key);
			})
		).then(function(jobs) {
			console.info('All jobs detais retrieved, building cache');
			var result = {};
			for (var i = 0; i < jobs.length; i++) {
				result[jobs[i].key] = jobs[i];
			}
			return result;
		});
	});
}

function getHashableText(name) {
	return name.replace(/[\s\.]+/g, '-').toLowerCase();
}

var _city_lookup = require('./city_lookup_map.json');
var _state_lookup = require('./state_lookup_map.json');
var _country_lookup = require('./country_lookup_map.json');
function getJobFromS3(log, s3, bucket, key) {
	return _q.ninvoke(s3, 'getObject', {
		Bucket: bucket,
		Key: key,
		ResponseContentEncoding: 'utf-8',
		ResponseContentType: 'application/json'
	}).then(function(data) {
		var rawJob = JSON.parse(data.Body);

		var job = {
			key: key,
			url: rawJob.SourceUri,
			company: rawJob.Company,
			title: rawJob.Title,
			html: rawJob.FullHtmlDescription,
			location: {
				raw: rawJob.Location
			}
		};

		var loc_parts = rawJob.Location.split(',');
		for (var i = 0; i < loc_parts.length; i++) {
			var loc = getHashableText(loc_parts[i]);
			var city, state;
			if (_city_lookup[loc]) {
				city = _city_lookup[loc];
				state = _state_lookup[city.state];
				job.city = city.canonicalName;
				job.state = state.canonicalName;
				job.country = _country_lookup[state.country].canonicalName;
				break;
			} else if (_state_lookup[loc]) {
				state = _state_lookup[loc];
				job.state = state.canonicalName;
				job.country = _country_lookup[state.country].canonicalName;
				break;
			} else if (_country_lookup[loc]) {
				job.country = _country_lookup[loc].canonicalName;
				break;
			}
		}

		// If we couldn't find the location, set the city to the unknown value
		if (typeof(job.country) === 'undefined') {
			_log.warn(
				{ location: rawJob.Location, id: key, company: rawJob.Company, title: rawJob.Title },
				'Location not found in lookup maps'
			);
			job.city = rawJob.Location;
		}

		return job;
	});
}

function addJobsToS3(log, s3, bucket, prefix, jobs) {
	var promises = [];
	for (var i = 0; i < jobs.length; i++) {
		promises.push(addJobToS3(log, s3, bucket, prefix, jobs[i]));
	}
	return _q.all(promises);
}

function addJobToS3(log, s3, bucket, prefix, job) {
	var normalized_company = job.Company.toLowerCase().replace(/[^a-z^0-9^\.^-]+/g, '');
	var uid = _hash(job);
	var key = prefix + normalized_company + '/' + uid + '.jd';

	// Check if item already exists
	if (typeof(_cache[key]) !== 'undefined') {
		log.info({ bucket: bucket, prefix: prefix, key: key }, 'Tried to job to s3, but already exists');
		return _q();
	}

	var options = {
		Bucket: bucket,
		Key: key,
		ACL: 'public-read',
		Body: JSON.stringify(job),
		CacheControl: 'max-age=' + (7 * 24 * 60 * 60),
		ContentEncoding: 'utf-8',
		ContentType: 'application/json'
	};

	log.info({ bucket: bucket, prefix: prefix, key: key }, 'Adding job to s3');
	return _q.ninvoke(s3, 'putObject', options).then(function(result) {
		log.info({ bucket: bucket, prefix: prefix, key: key }, 'Added job to s3');
	});
}

function init(log) {
	log.info('Initializing job store');

	if (typeof(_cache) !== 'undefined') {
		log.warn('Tried to initialize multiple times');
		return _q();
	}

	if (typeof(_config) === 'undefined') {
		_config = {
			accessKeyId: process.env.AWS_KEY_ID,
			secretAccessKey: process.env.AWS_KEY,
			region: process.env.AWS_REGION
		};
	}
	
	if (typeof(_config.accessKeyId) === 'undefined' ||
		typeof(_config.secretAccessKey) === 'undefined' ||
		typeof(_config.region) === 'undefined') {
		log.error(_config, 'Required config values not set');
		throw new Error('Must define environment variables AWS_KEY_ID, AWS_KEY, and AWS_REGION');
	}

	_aws.config.update(_config);

	log.info('Loading jobs from S3');
	var s3;
	return getS3Connection(_aws).then(function(s3_cxn) {
		s3 = s3_cxn;
		return s3BucketExists(log, s3, _s3_bucket);
	}).then(function(exists) {
		if (!exists) {
			return createS3Bucket(log, s3, _s3_bucket);
		}
	}).then(function() {
		return getAllJobsFromS3(log, s3, _s3_bucket, _s3_prefix);
	}).then(function(jobs) {
		log.info('Jobs loaded');
		_cache = jobs;
		var fs = require('fs');
		console.log('WRITING CACHE TO FILE');
		fs.writeFileSync('tmp.json', JSON.stringify(_cache));
	});
}

function add_jobs(log, jobs) {
	return getS3Connection(_aws).then(function(s3) {
		return addJobsToS3(log, s3, _s3_bucket, _s3_prefix, jobs);
	});
}

function remove_jobs(jobs) {
	throw new Error('Not implemented');
}

function query_jobs(query) {
	throw new Error('Not implemented');
}

module.exports = {
	init: init,
	add_jobs: add_jobs,
	remove_jobs: remove_jobs,
	query_jobs: query_jobs
};
