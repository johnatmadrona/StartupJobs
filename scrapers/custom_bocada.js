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
                var title = _util.scrub_string($(this).text());
                var anchor_url = _node_url.resolve(url, $(this).attr('href'));
                var description = $(this).parent().next();
            	var new_jd = {
	                url: anchor_url,
	                company: company,
	                title: title,
	                location: _util.map_location(log, location),
	                text: _util.scrub_string(description.text()),
	                html: description.html().trim()
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
