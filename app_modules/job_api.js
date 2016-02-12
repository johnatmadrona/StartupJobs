var _fs = require('fs');

function setup_jobs_api(log, express_app) {

	// List available logos
	express_app.get('/api/logos', function(req, res, next) {
		var dir_path = __dirname + '/images/logos/';
		var suffix = '-logo.png';
		_fs.readdir(dir_path, function(err, files) {
			if (err) {
				log.error(err, 'Error reading directory: ' + dir_path);
				res.status(500).send('Internal error');
				return next(err);
			}

			var result = [];
			for (var i = 0; i < files.length; i++) {
				var m = files[i].match(/(\w+)-logo.png$/);
				if (!m) {
					log.error('Unexpected file found in logos dir: ' + files[i]);
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
	express_app.get('/api/logos/:company_name', function(req, res, next) {
		if (/[\\\/\*\.]/.test(req.params.company_name)) {
			log.info('Bad user input, invalid company name: ' + req.params.company_name);
			res.status(400).send('Invalid request');
			return next();
		}

		var file_path = __dirname + '/images/logos/' + req.params.company_name + '-logo.png';
		_fs.stat(file_path, function(err, stats) {
			if (err) {
				log.error(err, 'Error retrieving file "' + file_path + '"');
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
					log.error(err, 'Error sending file "' + file_path + '"');
					res.status(500).end();
					return next(err);
				}

				log.info('Successfully sent file "' + file_path + '"');
				next();
			});
		});
	});

	express_app.get('/api/jobs', function(req, res) {
		res.status(501).send('Not implemented');
	});

	express_app.get('/api/jobs/:company_name', function(req, res) {
		res.status(501).send('Not implemented');
	});

	express_app.get('/api/jobs/:company_name/:job_id', function(req, res) {
		res.status(501).send('Not implemented');
	});
}

module.exports = {
	setup_jobs_api: setup_jobs_api
};
