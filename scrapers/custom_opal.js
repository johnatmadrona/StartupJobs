var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jds');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var jds = [];
            $('.career a').each(function() {
                var title = _util.scrub_string($(this).find('h3').text());
                var location = _util.scrub_string($(this).find('.location-name').text());
                var jd_url = _node_url.resolve(url, $(this).attr('href'));
	            jds.push(scrapeJobDescription(log, company, title, location, jd_url));
            });
            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, title, location, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jd');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var description = _cheerio.load('<span></span>')('span')
                .append($('.the-position'))
                .append($('.the-requirements'));
            var jd = _util.create_jd(
                log,
                url,
                company,
                title,
                location,
                _util.scrub_string(description.text()),
                description.html().trim()
            );
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
