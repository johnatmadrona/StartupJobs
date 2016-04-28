var _q = require('q');
var _bunyan = require('bunyan');

var _scraping_manager = require('./app_modules/scraping_manager');
var _scrapers = require('./scrapers');
var _job_store = require('./app_modules/job_store');
var _email = require('./app_modules/email');
var _s3 = require('./app_modules/s3');

var _config = (function() {
	var log_level = process.env.LOG_LEVEL;
	if (typeof(log_level) === 'undefined') {
		log_level = 'info';
	} else if (log_level !== 'trace' &&
		log_level !== 'debug' &&
		log_level !== 'info' &&
		log_level !== 'warn' &&
		log_level !== 'error' &&
		log_level !== 'fatal') {
		throw new Error('Log level must be one of the following string values: trace, debug, info, warn, error, fatal');
	}

	var aws_key_id = process.env.AWS_KEY_ID;
	if (typeof(aws_key_id) !== 'string' || aws_key_id.length < 1) {
		throw new Error('Must set a string value for AWS_KEY_ID');
	}
	var aws_key = process.env.AWS_KEY;
	if (typeof(aws_key) !== 'string' || aws_key.length < 1) {
		throw new Error('Must set a string value for AWS_KEY');
	}
	var aws_region = process.env.AWS_REGION;
	if (typeof(aws_region) !== 'string' || aws_region.length < 1) {
		throw new Error('Must set a string value for AWS_REGION');
	}
	var s3_bucket = process.env.S3_BUCKET;
	if (typeof(s3_bucket) !== 'string' || s3_bucket.length < 1) {
		throw new Error('Must set a string value for S3_BUCKET');
	}

	var email_host = process.env.EMAIL_HOST;
	if (typeof(email_host) !== 'string' || email_host.length < 1) {
		throw new Error('Must set a string value for EMAIL_HOST');
	}
	var email_port = Number.parseInt(process.env.EMAIL_PORT);
	if (!email_port || !Number.isInteger(email_port) ||
		email_port < 0 || email_port > 65535) {
		throw new Error('Must set EMAIL_PORT to an integer in the range of 0 to 65535');
	}
	var email_sender_address = process.env.EMAIL_SENDER_ADDRESS;
	if (typeof(email_sender_address) !== 'string' || email_sender_address.length < 1) {
		throw new Error('Must set a string value for EMAIL_SENDER_ADDRESS');
	}
	var email_sender_pwd = process.env.EMAIL_SENDER_PWD;
	if (typeof(email_sender_pwd) !== 'string' || email_sender_pwd.length < 1) {
		throw new Error('Must set a string value for EMAIL_SENDER_PWD');
	}
	var email_recipients = process.env.EMAIL_RECIPIENTS;
	if (typeof(email_recipients) !== 'string' || email_recipients.length < 1) {
		throw new Error('Must set a string value for EMAIL_RECIPIENTS');
	}

	return {
		log_level: log_level,
		aws_key_id: aws_key_id,
		aws_key: aws_key,
		aws_region: aws_region,
		s3_bucket: s3_bucket,
		email_host: email_host,
		email_port: email_port,
		email_sender_address: email_sender_address,
		email_sender_pwd: email_sender_pwd,
		email_recipients: email_recipients
	};
})();

var _log = new _bunyan({
	name: 'job-scraper',
	streams: [
		{
			stream: process.stdout,
			level: _config.log_level
		},
		{
			// If log size becomes an issue, can consider 
			// https://www.npmjs.com/package/bunyan-rotating-file-stream
			path: 'errors.log',
			level: 'error',
			type: 'rotating-file',
			period: '1d',
			count: 5
		}
	],
	serializers: _bunyan.stdSerializers
});

_s3.init(
	_log,
	_config.aws_key_id,
	_config.aws_key,
	_config.aws_region,
	_config.s3_bucket
).then(function() {
	_log.info('Uploading web UI');
	return _s3.create_website('web_ui/', 'index.html');
}).then(function() {
	_log.info('Initializing job store');
	return _job_store.init(_log, _s3);
}).then(function() {
	_log.info('Setting up emailer');
	return _email.create(
		_config.email_host,
		_config.email_port,
		_config.email_sender_address,
		_config.email_sender_pwd,
		_config.email_recipients
	);
}).then(function(emailer) {
	_log.info('Running scrapers');
	return _scraping_manager.run(
		_log,
		_job_store,
		_scrapers.scrapers,
		emailer
	);
}).done(function() {
	_log.info('Scraping complete');
}, function(err) {
	_log.error({ err: err }, 'Irrecoverable failure while scraping');
	throw err;
});
