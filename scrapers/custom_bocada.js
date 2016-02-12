var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jds');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var jds = [];
            $('.accordion > h3 > a').each(function() {
                var description = $(this).parent().next();
            	var new_jd = _util.create_jd(
                    log,
	                _node_url.resolve(url, $(this).attr('href')),
	                company,
	                _util.scrub_string($(this).text()),
	                location,
	                _util.scrub_string(description.text()),
	                description.html().trim()
	            );
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
