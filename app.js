var _q = require('q');
var _bunyan = require('bunyan');
var _uuid = require('uuid');
var _express = require('express');
var _body_parser = require('body-parser');
var _fs = require('fs');
var _hash = require('object-hash');

var _scrapers = require('./scrapers');
var _store = require('./app_modules/job_store');

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
app.use('/logos', _express.static(__dirname + '/images/logos'));

app.set('port', (process.env.PORT || 5000));

// List available logos
app.get('/api/logos', function(req, res, next) {
	var dir_path = __dirname + '/images/logos/';
	var suffix = '-logo.png';
	_fs.readdir(dir_path, function(err, files) {
		if (err) {
			_log.error(err, 'Error reading directory: ' + dir_path);
			res.status(500).send('Internal error');
			return next(err);
		}

		var result = [];
		for (var i = 0; i < files.length; i++) {
			var m = files[i].match(/(\w+)-logo.png$/);
			if (!m) {
				_log.error('Unexpected file found in logos dir: ' + files[i]);
			} else {
				var company_name = m[1];
				result.push({
					companyName: company_name,
					url: '/api/logos/' + company_name
				});
			}
		}

		res.json(result);
		next();
	});
});

// Retrieve logo
app.get('/api/logos/:company_name', function(req, res, next) {
	if (/[\\\/\*\.]/.test(req.params.company_name)) {
		_log.info('Bad user input, invalid company name: ' + req.params.company_name);
		res.status(400).send('Invalid request');
		return next();
	}

	var file_path = __dirname + '/images/logos/' + req.params.company_name + '-logo.png';
	_fs.stat(file_path, function(err, stats) {
		if (err) {
			_log.error(err, 'Error retrieving file "' + file_path + '"');
			if (err.code == 'ENOENT') {
				res.status(404).send('Not found');
				return next();
			} else {
				res.status(500).send('Internal error');
				return next(err);
			}
		}

		res.sendFile(file_path, { dotfiles: 'deny' }, function(err) {
			if (err) {
				_log.error(err, 'Error sending file "' + file_path + '"');
				res.status(500).end();
				return next(err);
			}

			_log.info('Successfully sent file "' + file_path + '"');
			next();
		});
	});
});

// TODO: Create API exposing scraped data
app.get('/api', function(req, res) {
	req.log.info({ body: req.body }, 'SMS content');

	//handleInboundSms(req.log, req.body.From, req.body.Body).done(function() {
		res.send('Ok');
	/*}, function(err) {
		req.log.error(err);
		res.status(500).send('Inernal error');
	});*/
});

function run_scrapers(log, scrapers) {
	log.info({ scraper_count: scrapers.length }, 'Initiating scraping');
	_store.init(
		log, 
		process.env.AWS_KEY_ID,
		process.env.AWS_KEY,
		process.env.AWS_REGION,
		'startup-jobs-v1'
	).then(function() {
		var ops = [];
		for (var i = 0; i < scrapers.length; i++) {
			ops.push(run_scraper(log, scrapers[i]));
		}
		return _q.allSettled(ops);
	}).done(function(results) {
		var success_count = results.reduce(function(p, item) {
			return p + (item.state === 'rejected' ? 0 : 1);
		}, 0);

		var status = {
			total_count: results.length,
			success_count: success_count,
			failure_count: results.length - success_count
		};

		if (status.failure_count > 0) {
			log.error(status, 'Scraping complete, with errors');
		} else {
			log.info(status, 'Scraping complete, without errors');
		}
	}, function(err) {
		log.error(err, 'Error while running scrapers');
	});
}

function run_scraper(log, scraper) {
	log.info({ company: scraper.company }, 'Running scraper');
	return scraper.scrape(log)
		.then(function(jds) { return store_jobs(log, jds); })
		.then(function(jds) { return get_obsolete_jobs(scraper.company, jds); })
		.then(function(obsolete_jobs) {
			log.info({ company: scraper.company, count: obsolete_jobs.length }, 'Deleting obsolete jobs');
			return _store.remove_jobs(log, obsolete_jobs);
		}).catch(function(err) {
			log.error({ company: scraper.company, err: err }, 'Error while running scraper');
			return _q.reject(err);
		});
}

function store_jobs(log, jds) {
	return _store.add_jobs(log, jds).then(function() {
		return jds;
	});
}

function get_obsolete_jobs(company, jds) {
	var new_jobs_map = {};
	for (var i = 0; i < jds.length; i++) {
		new_jobs_map[_hash(jds[i])] = true;
	}
	return _store.query_jobs(_log, { company: company }).then(function(old_jobs) {
		// Create a list of stored jobs that are obsolete
		return old_jobs.filter(function(old_job) {
			return typeof(new_jobs_map[_hash(old_job)]) === 'undefined';
		});
	});
}

run_scrapers(_log, _scrapers.scrapers);

//app.listen(app.get('port'), function() {
//	_log.info('Express server started on port ' + app.get('port'));
//});
