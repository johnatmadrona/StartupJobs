var _q = require('q');
var _hash = require('object-hash');
var _moment = require('moment');

function run_scrapers(log, store, scrapers) {
	return _q().then(function() {
		log.info({ scraper_count: scrapers.length }, 'Initiating scraping');
		var ops = [];
		for (var i = 0; i < scrapers.length; i++) {
			ops.push(run_scraper(log, store, scrapers[i]));
		}
		return _q.allSettled(ops);
	}).then(function(results) {
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

function run_scraper(log, store, scraper) {
	log.info({ company: scraper.company }, 'Running scraper');
	return scraper.scrape(log)
		.then(function(jds) { return store_jobs(log, store, jds); })
		.then(function(jds) { return get_obsolete_jobs(log, store, scraper.company, jds); })
		.then(function(obsolete_jobs) {
			log.info({ company: scraper.company, count: obsolete_jobs.length }, 'Deleting obsolete jobs');
			return store.remove_jobs(log, obsolete_jobs);
		}).catch(function(err) {
			log.error({ company: scraper.company, err: err }, 'Error while running scraper');
			return _q.reject(err);
		});
}

function store_jobs(log, store, jds) {
	return store.add_jobs(log, store, jds).then(function() {
		return jds;
	});
}

function get_obsolete_jobs(log, store, company, jds) {
	var new_jobs_map = {};
	for (var i = 0; i < jds.length; i++) {
		new_jobs_map[_hash(jds[i])] = true;
	}
	var query = { operation: 'jobs-by-company', company: company };
	return store.query(log, query).then(function(old_jobs) {
		// Create a list of stored jobs that are obsolete
		return old_jobs.filter(function(old_job) {
			return typeof(new_jobs_map[_hash(old_job)]) === 'undefined';
		});
	});
}

var _last_time_key = 'last-run-time';
function schedule_scraping(log, job_store, value_store, scrapers, recurring_hour_of_day) {
	return get_ms_until_next_run(log, value_store, recurring_hour_of_day).then(function(ms_until_next_time) {
		setTimeout(function() {
			schedule_scraping(
				log,
				job_store,
				value_store,
				scrapers,
				recurring_hour_of_day
			).then(function() {
				return run_scrapers(log, job_store, scrapers);
			}).then(function() {
				return value_store.add_value(log, _last_time_key, _moment());
			}).done(function() {
				log.debug('Updated last scrape time');
			}, function(err) {
				log.error({ err: err }, 'Error while scheduling scraping');
			});
		}, ms_until_next_time);

		var next_scrape_time = _moment().add(ms_until_next_time, 'ms');
		log.info({ next_scrape_time: next_scrape_time }, 'Timer set for next scraping');
	});
}

function get_ms_until_next_run(log, value_store, desired_hour_of_day) {
	return value_store.get_value(log, _last_time_key).then(function(last_time_str) {
		if (last_time_str === null) {
			return 0;
		}

		var last_time = _moment(last_time_str);
		var next_time = _moment([last_time.year(), last_time.month(), last_time.date()])
			.add(1, 'd')
			.add(desired_hour_of_day, 'h');

		return next_time.valueOf() - last_time.valueOf();
	});
}

module.exports = {
	run: run_scrapers,
	schedule: function(log, job_store, value_store, scrapers, recurring_hour_of_day) {
		return schedule_scraping(
			log,
			job_store,
			value_store,
			scrapers.scrapers,
			recurring_hour_of_day
		);
	}
};
