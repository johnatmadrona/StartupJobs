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
            $('.entry-content a[href^="https://jobs.pro.com/?page_id="]').each(function() {
	            jds.push(scrapeJobDescription(log, company, $(this).attr('href')));
            });
            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jd');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var header = $('.entry-title').text();
            var header_divide = header.indexOf(',');
            var title = _util.scrub_string(header.substring(0, header_divide));
            var location = _util.scrub_string(header.substring(header_divide + 1));
            var description = $('.entry-content');

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
