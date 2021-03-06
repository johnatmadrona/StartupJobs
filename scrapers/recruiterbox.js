var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, rb_id, public_url) {
    var url = 'https://app.recruiterbox.com/widget/' + rb_id + '/openings/';
    log.info({ company: company, rb_id: rb_id, public_url: public_url }, 'Getting jd links');
    return _util.request(log, url).then(function(rbox_jobs_text) {
        var jobs = JSON.parse(rbox_jobs_text);
        var jds = [];
        for (var i = 0; i < jobs.length; i++) {
            var location = '';
            if (jobs[i].location.city !== null && jobs[i].location.city.length > 0) {
                location += jobs[i].location.city + ', ';
            }
            if (jobs[i].location.state !== null && jobs[i].location.state.length > 0) {
                location += jobs[i].location.state + ', ';
            }
            if (jobs[i].location.country !== null && jobs[i].location.country.length > 0) {
                location += jobs[i].location.country;
            }
            if (location.endsWith(', ')) {
                location = location.substring(0, location.length - 2).trim();
            }

            var $ = _cheerio.load('<span></span>')('span').append(jobs[i].description);

            var new_jd = _util.create_jd(
                log,
                public_url,
                company,
                jobs[i].title,
                location,
                _util.scrub_string($.text()),
                $.html().trim()
            );
            jds.push(new_jd);
        }

        return _q.all(jds);
    });
}

module.exports = {
    scrape: scrape
};
