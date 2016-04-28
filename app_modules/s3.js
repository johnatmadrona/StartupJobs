var _q = require('q');
var _aws = require('aws-sdk');
var _fs = require('fs');
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
	if (typeof(value) === 'undefined' || value === null) {
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
	return get_s3_connection(_aws).then(function(s3) {
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

function list(log, prefix, start_key) {
	var options = {
		Bucket: _config.s3_bucket
	};
	if (typeof(prefix) !== 'undefined' && prefix !== null) {
		options.Prefix = prefix;
	}
	if (typeof(start_key) !== 'undefined' && start_key !== null) {
		options.Marker = start_key;
	}

	log.debug({ options: options }, 'Listing s3 objects');
	return get_s3_connection(_aws).then(function(s3) {
		return _q.ninvoke(s3, 'listObjects', options);
	}).then(function(initial_result) {
		var next_op = _q();
		if (initial_result.IsTruncated) {
			var last_key = initial_result.Contents[initial_result.Contents.length - 1].Key;
			next_op = list(log, prefix, last_key);
		}
		return next_op.then(function(next_result) {
			var ops = next_result || [];
			return _q.all(ops.concat(initial_result.Contents));
		});
	});
}

function get_content_info(type) {
	switch (type) {
		case 'html':
			return {
				type: 'text/html',
				encoding: 'utf8'
			};
		case 'css':
			return {
				type: 'text/css',
				encoding: 'utf8'
			};
		case 'js':
			return {
				type: 'application/x-javascript',
				encoding: 'utf8'
			};
		case 'png':
			return {
				type: 'image/png',
				encoding: null
			};
		case 'ico':
			return {
				type: 'image/x-icon',
				encoding: null
			};
	}

	throw new Error('Unhandled type: "' + type + '"');
}

function upload_dir(s3, src_dir, dst_dir) {
	return _q.ninvoke(_fs, 'readdir', src_dir).then(function(files) {
		var ops = [];
		files.forEach(function(file) {
			var src_path = src_dir + file;
			var dst_path = dst_dir + file;
			var op = _q.ninvoke(_fs, 'stat', src_path).then(function(file_stat) {
				if (file_stat.isDirectory()) {
					return upload_dir(s3, src_path + '/', dst_path + '/');
				} else {
					var content_info = get_content_info(file.match(/\.(\w+)$/)[1]);
					return _q.ninvoke(_fs, 'readFile', src_path, content_info.encoding).then(function(content) {
						var options = {
							Bucket: _config.s3_bucket,
							Key: dst_path,
							ACL: 'public-read',
							Body: content,
							CacheControl: 'max-age=' + (7 * 24 * 60 * 60),
							ContentType: content_info.type
						};
						return _q.ninvoke(s3, 'putObject', options);
					});
				}
			});
			ops.push(op);
		});
		return _q.all(ops);
	});
}

function create_website(src_dir, index_doc) {
	return get_s3_connection(_aws).then(function(s3_cxn) {
		return upload_dir(s3_cxn, src_dir + '/', '').then(function() { return s3_cxn; });
	}).then(function(s3_cxn) {
		var options = {
			Bucket: _config.s3_bucket,
			WebsiteConfiguration: {
				IndexDocument: {
					Suffix: index_doc
				}
			}
		};
		return _q.ninvoke(s3_cxn, 'putBucketWebsite', options);
	});
}

module.exports = {
	init: init,
	upsert: upsert,
	retrieve: retrieve,
	remove: remove,
	list: list,
	create_website: create_website
};
