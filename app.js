var _q = require('q');
var _bunyan = require('bunyan');
var _uuid = require('uuid');
var _express = require('express');
var _body_parser = require('body-parser');

var _job_api = require('./app_modules/job_api');
var _scraping_manager = require('./app_modules/scraping_manager');
var _scrapers = require('./scrapers');
var _job_store = require('./app_modules/job_store');
var _value_store = require('./app_modules/value_store');
var _email = require('./app_modules/email');

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

	var hour_of_day_to_scrape_utc = Number.parseInt(process.env.HOUR_OF_DAY_TO_SCRAPE_UTC);
	if (!hour_of_day_to_scrape_utc || !Number.isInteger(hour_of_day_to_scrape_utc) ||
		hour_of_day_to_scrape_utc < 0 || hour_of_day_to_scrape_utc > 23) {
		throw new Error('Must set HOUR_OF_DAY_TO_SCRAPE_UTC to an integer in the range of 0 to 23');
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
		hour_of_day_to_scrape_utc: hour_of_day_to_scrape_utc,
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
			period: '1w',
			count: 3
		}
	],
	serializers: _bunyan.stdSerializers
});

var app = _express();
app.use(_body_parser.json());
app.use(_body_parser.urlencoded({ extended: true }));
app.use(function(req, res, next) {
	req.id = _uuid.v4();
	req.log = _log.child({ req_id: req.id });
	res.setHeader('X-Request-Id', req.id);

	_log.info({ req_id: req.id, req: req }, 'Received request');

	res.on('finish', function() {
		req.log.info({
			headers: res._headers,
			statusCode: res.statusCode,
			statusMessage: res.statusMessage
		}, 'Response sent');
	});
	res.on('close', function() {
		req.log.info('Connection terminated');
	});

	next();
});
app.use(_express.static(__dirname + '/WebUI'));
app.use('/logos', _express.static(__dirname + '/images/logos'));

app.set('port', (process.env.PORT || 5000));

_log.info('Initializing job store');
_job_store.init(
	_log,
	_config.aws_key_id,
	_config.aws_key,
	_config.aws_region,
	_config.s3_bucket
).then(function() {
	_log.info('Initializing value store');
	return _value_store.init(
		_log,
		_config.aws_key_id,
		_config.aws_key,
		_config.aws_region,
		_config.s3_bucket
	);
}).then(function() {
	return _email.create(
		_config.email_host,
		_config.email_port,
		_config.email_sender_address,
		_config.email_sender_pwd,
		_config.email_recipients
	);
}).then(function(emailer) {
	_log.info('Scheduling job scraping');
	return _scraping_manager.schedule(
		_log,
		_job_store,
		_value_store,
		_scrapers.scrapers,
		emailer,
		_config.hour_of_day_to_scrape_utc
	);
}).then(function() {
	_log.info('Scraping scheduled, setting up jobs api');
	_job_api.setup_jobs_api(app, _job_store);
}).then(function() {
	app.listen(app.get('port'), function() {
		_log.info('Express server started on port ' + app.get('port'));
	});
}).done(function() {
	_log.info('Setup complete');
}, function(err) {
	_log.error({ err: err }, 'Irrecoverable failure while setting up scraping');
	throw err;
});
