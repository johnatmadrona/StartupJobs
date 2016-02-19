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
	return scraper.scrape(log).then(function(jds) {
		return store_jobs(log, store, jds);
	}).then(function(jds) {
		return get_obsolete_jobs(log, store, scraper.company, jds);
	}).then(function(obsolete_jobs) {
		if (obsolete_jobs.length > 0) {
			log.info({ company: scraper.company, count: obsolete_jobs.length }, 'Deleting obsolete jobs');
			return store.remove_jobs(log, obsolete_jobs);
		}
	}).catch(function(err) {
		log.error({ company: scraper.company, err: err }, 'Error while running scraper');
		return _q.reject(err);
	});
}

function store_jobs(log, store, jds) {
	return store.add_jobs(log, jds).then(function() {
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

var _last_time_utc_key = 'last-run-time-utc';
function schedule_scraping(log, job_store, value_store, scrapers, recurring_hour_of_day_utc) {
	setTimeout(function() {
		schedule_scraping(
			log,
			job_store,
			value_store,
			scrapers,
			recurring_hour_of_day_utc
		);
	}, 15 * 60 * 1000);

	return is_time_for_next_run(log, value_store, recurring_hour_of_day_utc).then(function(do_run) {
		if (do_run) {
			return run_scrapers(log, job_store, scrapers).then(function() {
				return value_store.add_value(log, _last_time_utc_key, _moment.utc());
			});
		}
	}).done(function() {
		log.debug('Scraping complete');
	}, function(err) {
		log.error({ err: err }, 'Error while scheduling scraping');
	});
}

function is_time_for_next_run(log, value_store, desired_hour_of_day_utc) {
	return value_store.get_value(log, _last_time_utc_key).then(function(last_time_str) {
		if (last_time_str === null) {
			return true;
		}

		var last_time = _moment.utc(last_time_str);
		var day_of_last_time = _moment.utc([last_time.year(), last_time.month(), last_time.date()]);
		var now = _moment.utc();
		var today = _moment.utc([now.year(), now.month(), now.date()]);
		var target_time_today = today.clone().add(desired_hour_of_day_utc, 'h');

		// If the last run happened today, but occurred before the 
		// desired hour, we need to run again after the desired hour
		var need_to_run_again_today = 
			day_of_last_time.diff(today) === 0 && 
			last_time.diff(target_time_today) < 0 && 
			now.diff(target_time_today) >= 0;

		// If the last run happened before today, we need to run 
		// after the desired hour today
		var need_to_run_first_time_today = 
			day_of_last_time.diff(today) < 0 && 
			now.diff(target_time_today) >= 0;

		return need_to_run_again_today || need_to_run_first_time_today;
	});
}

module.exports = {
	run: run_scrapers,
	schedule: function(log, job_store, value_store, scrapers, recurring_hour_of_day_utc) {
		setTimeout(function() {
			schedule_scraping(
				log,
				job_store,
				value_store,
				scrapers.scrapers,
				recurring_hour_of_day_utc
			);
		}, 0);
	}
};
