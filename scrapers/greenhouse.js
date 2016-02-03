var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, gh_id) {
    var url = 'https://boards.greenhouse.io/embed/job_board?for=' + gh_id;
    var d = _q.defer();

    log.info({ company: company, gh_id: gh_id, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var links = $('.opening > a');
            var jds = [];
            links.each(function() {
                var jd_home_url = $(this).attr('href');
                var jd_id = /gh_jid=(\d+)/.exec(jd_home_url)[1];
                jds.push(scrapeJobDescription(log, company, jd_home_url, gh_id, jd_id));
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, jd_home_url, gh_id, jd_id) {
    var url = 'https://boards.greenhouse.io/embed/job_app?for=' + gh_id + '&token=' + jd_id;
    log.info({
        company: company,
        jd_home_url: jd_home_url,
        gh_id: gh_id,
        jd_id: jd_id,
        url: url
    }, 'Getting jd');

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var loc = _util.scrub_string($('.location').text());
            var jd = {
                url: jd_home_url,
                company: company,
                title: _util.scrub_string($('.app-title').text()),
                location: _util.map_location(log, loc),
                text: _util.scrub_string($('.content').text()),
                html: $('.content').html()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
