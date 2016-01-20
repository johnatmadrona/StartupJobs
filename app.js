var _bunyan = require('bunyan');
var _uuid = require('uuid');
var _express = require('express');
var _body_parser = require('body-parser');

var _scrapers = require('./scrapers');

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

//app.listen(app.get('port'), function() {
//	_log.info('Express server started on port ' + app.get('port'));
	console.log('SCRAPING...');
	_scrapers.jobvite.scrape(_log, 'Animoto', 'animoto').done(function(jds) {
		console.log(jds);
	}, function(err) {
		console.log(err);
	});
//});

