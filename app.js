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

var _log = new _bunyan({
	name: 'job-scraper',
	streams: [
		{
			stream: process.stdout,
			level: 'debug'
		},
		{
			path: 'errors.log',
			level: 'error'
		}
	],
	serializers: _bunyan.stdSerializers
});

var _config = (function() {
	var aws_key_id = process.env.AWS_KEY_ID;
	if (!aws_key_id || aws_key_id.length < 1) {
		throw new Error('Must set AWS_KEY_ID');
	}
	var aws_key = process.env.AWS_KEY;
	if (!aws_key || aws_key.length < 1) {
		throw new Error('Must set AWS_KEY');
	}
	var aws_region = process.env.AWS_REGION;
	if (!aws_region || aws_region.length < 1) {
		throw new Error('Must set AWS_REGION');
	}
	var s3_bucket = process.env.S3_BUCKET;
	if (!s3_bucket || s3_bucket.length < 1) {
		throw new Error('Must set S3_BUCKET');
	}
	var hour_of_day_to_scrape = Number.parseInt(process.env.HOUR_OF_DAY_TO_SCRAPE);
	if (!hour_of_day_to_scrape || !Number.isInteger(hour_of_day_to_scrape) ||
		hour_of_day_to_scrape < 0 || hour_of_day_to_scrape > 23) {
		throw new Error('Must set HOUR_OF_DAY_TO_SCRAPE to an integer in the range of 0 to 23');
	}

	return {
		aws_key_id: process.env.AWS_KEY_ID,
		aws_key: process.env.AWS_KEY,
		aws_region: process.env.AWS_REGION,
		s3_bucket: s3_bucket, //'startup-jobs-v1',
		hour_of_day_to_scrape: hour_of_day_to_scrape
	};
})();

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
	_log.info('Scheduling job scraping');
	return _scraping_manager.schedule(
		_log,
		_job_store,
		_value_store,
		_scrapers,
		_config.hour_of_day_to_scrape
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
