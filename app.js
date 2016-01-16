var bunyan = require('bunyan');
var uuid = require('uuid');
var express = require('express');
var bodyParser = require('body-parser');

// TODO: Scrape job pages
//var scrapers = require('./scrapers/test.js');

var _log = new bunyan({
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
	serializers: bunyan.stdSerializers
});

var app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(function(req, res, next) {
	req.id = uuid.v4();
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

app.use(express.static(__dirname + '/WebUI'));

app.set('port', (process.env.PORT || 5000));

// TODO: Create API exposing scraped data
// TODO: Create API exposing company logos
app.get('/api', function(req, res) {
	req.log.info({ body: req.body }, 'SMS content');

	//handleInboundSms(req.log, req.body.From, req.body.Body).done(function() {
		res.send('Ok');
	/*}, function(err) {
		req.log.error(err);
		res.status(500).send('Inernal error');
	});*/
});

app.listen(app.get('port'), function() {
	_log.info('Express server started on port ' + app.get('port'));
});
