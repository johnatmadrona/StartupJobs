var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jds');
    _request({ url: url, headers: { 'User-Agent': 'startup-jobs' } }, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var jds = [];
            $('*[data-type="page"] *[data-block-type="23"]').each(function() {
                var title = _util.scrub_string($(this).text());
            	var new_jd = {
	                url: url,
	                company: company,
	                title: title,
	                location: _util.map_location(log, location),
	                text: _util.scrub_string($(this).next().text()),
	                html: $(this).next().html().trim()
	            };
	            jds.push(new_jd);
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
