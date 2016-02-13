var _fs = require('fs');

function setup_jobs_api(express_app, job_store) {

	// List available logos
	express_app.get('/api/logos', function(req, res, next) {
		var dir_path = __dirname + '/../images/logos/';
		var suffix = '-logo.png';
		_fs.readdir(dir_path, function(err, files) {
			if (err) {
				req.log.error(err, 'Error reading directory: ' + dir_path);
				res.status(500).send('Internal error');
				return next(err);
			}

			var result = [];
			for (var i = 0; i < files.length; i++) {
				var m = files[i].match(/(\w+)-logo.png$/);
				if (!m) {
					req.log.error('Unexpected file found in logos dir: ' + files[i]);
				} else {
					var company_name = m[1];
					result.push({
						company_name: company_name,
						url: '/api/logos/' + company_name
					});
				}
			}

			res.json(result);
			next();
		});
	});

	// Retrieve logo
	express_app.get('/api/logos/:company_name', function(req, res, next) {
		if (/[\\\/\*\.]/.test(req.params.company_name)) {
			req.log.info('Bad user input, invalid company name: ' + req.params.company_name);
			res.status(400).send('Invalid request');
			return next();
		}

		var file_path = __dirname + '/../images/logos/' + req.params.company_name + '-logo.png';
		_fs.stat(file_path, function(err, stats) {
			if (err) {
				req.log.error(err, 'Error retrieving file "' + file_path + '"');
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
					req.log.error(err, 'Error sending file "' + file_path + '"');
					res.status(500).end();
					return next(err);
				}

				req.log.info('Successfully sent file "' + file_path + '"');
				next();
			});
		});
	});

	express_app.get('/api/companies', function(req, res, next) {
		var query = { operation: 'list-companies' };
		job_store.query(req.log, query).done(function(companies) {
			var result = companies.map(function(company_info) {
				return {
					company_name: company_info.name,
					company_url: '/api/companies/' + encodeURIComponent(company_info.name),
					jobs_url: '/api/companies/' + encodeURIComponent(company_info.name) + '/jobs'
				};
			});
			res.json(result);
			next();
		}, function(err) {
			req.log.error({ err: err }, 'Error querying job store');
			res.status(500).send('Internal error');
			next(err);
		});
	});

	express_app.get('/api/companies/:company_name', function(req, res, next) {
		res.status(501).send('Not implemented');
	});

	express_app.get('/api/companies/:company_name/jobs', function(req, res, next) {
		if (/[\\\/\*\.]/.test(req.params.company_name)) {
			req.log.info('Bad user input, invalid company name: ' + req.params.company_name);
			res.status(400).send('Invalid request');
			return next();
		}

		var query = { operation: 'jobs-by-company', company: req.params.company_name };
		job_store.query(req.log, query).done(function(jobs) {
			res.json(jobs);
			next();
		}, function(err) {
			req.log.error({ err: err }, 'Error querying job store');
			res.status(500).send('Internal error');
			next(err);
		});
	});

	express_app.get('/api/companies/:company_name/jobs/:job_id', function(req, res, next) {
		res.status(501).send('Not implemented');
	});
}

module.exports = {
	setup_jobs_api: setup_jobs_api
};
