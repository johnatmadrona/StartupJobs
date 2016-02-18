var _q = require('q');
var _aws = require('aws-sdk');
var _config;

function get_s3_connection(aws) {
	return _q().then(function() { return new aws.S3(); });
}

function s3_bucket_exists(log, s3, bucket) {
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

function create_s3_bucket(log, s3, bucket) {
	var options = {
		Bucket: bucket,
		ACL: 'public-read',
		CreateBucketConfiguration: {
			LocationConstraint: _config.aws_region
		}
	};

	log.debug({ bucket: bucket }, 'Creating s3 bucket');
	return _q.ninvoke(s3, 'createBucket', options).then(function(result) {
		log.info({ bucket: bucket }, 'Created s3 bucket');
		return true;
	});
}

function init(log, aws_key_id, aws_key, aws_region, s3_bucket) {
	log.info(
		{
			aws_key_id: aws_key_id ? '(set)' : aws_key_id,
			aws_key: aws_key ? '(set)' : aws_key,
			aws_region: aws_region ? '(set)' : aws_region,
			s3_bucket: s3_bucket
		},
		'Initializing S3'
	);

	if (!aws_key_id || !aws_key || !aws_region || !s3_bucket) {
		log.error('Missing required parameter');
		return _q.reject(new Error('Missing required parameter'));
	}

	if (!_config) {
		_config = {
			aws_key_id: aws_key_id,
			aws_key: aws_key,
			aws_region: aws_region,
			s3_bucket: s3_bucket
		};
	}

	_aws.config.update({
		accessKeyId: _config.aws_key_id,
		secretAccessKey: _config.aws_key,
		region: _config.aws_region
	});

	var s3;
	return get_s3_connection(_aws).then(function(s3_cxn) {
		s3 = s3_cxn;
		return s3_bucket_exists(log, s3, _config.s3_bucket);
	}).then(function(exists) {
		if (!exists) {
			return create_s3_bucket(log, s3, _config.s3_bucket);
		}
	}).then(function() {
		log.info('S3 successfully initialized');
	});
}

function upsert(log, key, value) {
	if (typeof(key) !== 'string' || key === null || key.length < 1) {
		return _q.reject(new Error('Must provide a string value for parameter "key"'));
	}
	if (typeof(value) === 'undefined') {
		return _q.reject(new Error('Must supply a value for parameter "value"'));
	}

	var options = {
		Bucket: _config.s3_bucket,
		Key: key,
		ACL: 'public-read',
		Body: JSON.stringify(value),
		CacheControl: 'max-age=' + (7 * 24 * 60 * 60),
		ContentEncoding: 'utf-8',
		ContentType: 'application/json'
	};
	log.debug({ options: options }, 'Writing value to s3');
	return get_s3_connection(aws).then(function(s3) {
		return _q.ninvoke(s3, 'putObject', options);
	});
}

function retrieve(log, key) {
	if (typeof(key) !== 'string' || key === null || key.length < 1) {
		return _q.reject(new Error('Must provide a string value for parameter "key"'));
	}

	var options = {
		Bucket: _config.s3_bucket,
		Key: key,
		ResponseContentEncoding: 'utf-8',
		ResponseContentType: 'application/json'
	};
	log.debug({ options: options }, 'Retrieving value from s3');
	return get_s3_connection(_aws).then(function(s3) {
		return _q.ninvoke(s3, 'getObject', options).then(function(data) {
			return JSON.parse(data.Body);
		});
	});
}

function remove(log, keys) {
	if (!keys) {
		return _q.reject(new Error('Must provide an array of string values for parameter "keys"'));
	}
	if (keys.length < 1) {
		return _q();
	}
	// The AWS SDK has a limit of 1000 keys for the deleteObjects request
	if (keys.length > 1000) {
		return _q.reject(new Error('Removal of more than 1000 keys not yet implemented'));
	}

	var prepared_keys = keys.map(function(item) {
		return { Key: item };
	});
	var options = {
		Bucket: _config.s3_bucket,
		Delete: {
			Objects: prepared_keys
		}
	};

	log.debug({ options: options }, 'Removing value from s3');
	return get_s3_connection(_aws).then(function(s3) {
		return _q.ninvoke(s3, 'deleteObjects', options);
	});
}

function list(log, prefix, max_count) {
	var options = {
		Bucket: _config.s3_bucket
	};
	if (typeof(prefix) !== 'undefined' && prefix !== null) {
		options.Prefix = prefix;
	}
	if (typeof(max_count) !== 'undefined') {
		options.MaxKeys = max_count;
	}

	log.debug({ options: options }, 'Listing s3 objects');
	return get_s3_connection(_aws).then(function(s3) {
		return _q.ninvoke(s3, 'listObjects', options);
	}).then(function(result) {
		if ((typeof(max_count) === 'undefined' || result.Contents.length < max_count) && result.IsTruncated) {
			//var nextMarker = result.Contents[data.Contents.length - 1].Key;
			return _q.reject(new Error('Truncation handling not yet implemented'));
		}
		return _q.all(result.Contents);
	});
}

module.exports = {
	init: init,
	upsert: upsert,
	retrieve: retrieve,
	remove: remove,
	list: list
};
