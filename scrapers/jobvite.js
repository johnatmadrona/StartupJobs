var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, jvId) {
    var url = 'http://jobs.jobvite.com/careers/' + jvId + '/jobs?jvi=';
    var d = _q.defer();

    log.info({ company: company, jvId: jvId, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var jobPagePrefix = '/' + jvId + '/job/';
            var $ = _cheerio.load(html);
            var links = $('a[href^="' + jobPagePrefix + '"]');

            var jds = [];
            links.each(function() {
                var jdUrl = _node_url.resolve(url, $(this).attr('href'));
                jds.push(scrapeJobDescription(log, company, jdUrl));
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var loc = _util.scrub_string($('.jv-job-detail-meta').contents()[2].data);
            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('.jv-header').text()),
                location: _util.map_location(log, loc),
                text: _util.scrub_string($('.jv-job-detail-description').text()),
                html: $('.jv-job-detail-description').html()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
