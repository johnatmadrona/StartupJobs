var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
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
            $('.careersContainer > a').each(function() {
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
            var jd = _util.create_jd(
                log,
                url,
                company,
                _util.scrub_string($('.updateIntro > h1').text()),
                _util.scrub_string($('.updateIntro > .meta').text().split('\u2022')[0]),
                _util.scrub_string($('.jobInfo').text()),
                $('.jobInfo').html().trim()
            );
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
