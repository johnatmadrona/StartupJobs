var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, jobaline_id) {
    var d = _q.defer();

    var url = 'http://' + jobaline_id + '.jobaline.com';

    log.info({ company: company, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else if (url !== res.request.uri.href) {
            var jds = [];
            var $ = _cheerio.load(html);
            $('a.search_job_title').each(function() {
                jds.push(scrape_job_description(log, company, $(this).attr('href')));
            });
            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrape_job_description(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');

    var d = _q.defer();
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var raw_location = _util.scrub_string($('.info-icons').children().eq(0).text());
            var parsed_location = /([\w ,]+)\d{5}/.exec(raw_location)[1].trim();
            var content_node = $('.job-description-container');
            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('.job-title').text()),
                location: _util.map_location(log, parsed_location),
                text: _util.scrub_string(content_node.text()),
                html: content_node.html().trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
