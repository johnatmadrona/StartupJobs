var _q = require('q');
var _hash = require('object-hash');
var _moment = require('moment');

function run_scrapers(log, store, scrapers) {
	_q().then(function() {
		log.info({ scraper_count: scrapers.length }, 'Initiating scraping');
		var ops = [];
		for (var i = 0; i < scrapers.length; i++) {
			ops.push(run_scraper(log, store, scrapers[i]));
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

function schedule_scraping(log, store, scrapers, last_time, recurring_hour_of_day) {
	var next_time = get_next_daily_time(last_time, recurring_hour_of_day);
	var ms_until_next_time = next_time.valueOf() - last_time.valueOf();

	setTimeout(function() {
		schedule_scraping(
			log,
			store,
			scrapers,
			next_time,
			recurring_hour_of_day
		);
		run_scrapers(log, store, scrapers);
	}, ms_until_next_time);

	log.info({ next_time: next_time }, 'Scraping scheduled');
}

function get_next_daily_time(base_time, desired_hour_of_day) {
	return _moment([base_time.year(), base_time.month(), base_time.date()])
		.add(1, 'd')
		.add(desired_hour_of_day, 'h');
}

module.exports = {
	run: run_scrapers,
	schedule: function(log, store, scrapers, recurring_hour_of_day) {
		schedule_scraping(log, store, scrapers.scrapers, _moment(), recurring_hour_of_day);
	}
};
