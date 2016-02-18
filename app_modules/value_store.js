var _q = require('q');
var _s3 = require('./s3');

module.exports = {
	init: function(log, aws_key_id, aws_key, aws_region, s3_bucket) {
		log.debug(
			{
				aws_key_id: aws_key_id ? '(set)' : aws_key_id,
				aws_key: aws_key ? '(set)' : aws_key,
				aws_region: aws_region ? '(set)' : aws_region,
				s3_bucket: s3_bucket
			},
			'Initializing value store'
		);
		return _s3.init(log, aws_key_id, aws_key, aws_region, s3_bucket);
	},
	add_value: function(log, key, value) {
		log.debug({ key: key, value: value }, 'Adding value to s3');
		return _s3.upsert(log, key, value);
	},
	get_value: function(log, key) {
		log.debug({ key: key }, 'Getting value from s3');
		return _s3.retrieve(log, key).catch(function(err) {
			if (err.code && err.code === 'NoSuchKey') {
				return null;
			}
			throw err;
		});
	},
	remove_value: function(log, key)  {
		log.debug({ key: key }, 'Removing value from s3');
		return _s3.remove(log, key);
	}
};
