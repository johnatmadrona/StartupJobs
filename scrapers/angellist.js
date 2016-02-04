var _q = require('q');
var _node_url = require('url');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

var _al_headers = {
    'Accept': 'text/html',
    'Cache-Control': 'max-age=0',
    'Host': 'angel.co',
    'User-Agent': 'startup-jobs'
};

function scrape(log, company, angellist_id) {
    var d = _q.defer();

    var options = {
        url: 'https://angel.co/' + angellist_id + '/jobs/',
        headers: _al_headers
    };

    log.info({ company: company, angellist_id: angellist_id, url: options.url }, 'Getting jd links');
    _request.get(options, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var listings = $('.listing');
            var jds = [];

            log.info('Found ' + listings.length + ' listings');
            listings.each(function() {
                var titleNode = $(this).find('.title > a');
                var locationNode = $(this).find('.job-data').eq(1);
                jds.push(scrapeJobDescription(
                    log,
                    company,
                    _util.scrub_string(titleNode.text()),
                    _util.scrub_string(locationNode.text()),
                    _node_url.resolve(options.url, titleNode.attr('href'))
                ));
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, title, location, url) {
    var d = _q.defer();

    log.info({ company: company, title: title, location: location, url: url }, 'Getting jd');
    _request({ url: url, headers: _al_headers }, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var jd = {
                url: url,
                company: company,
                title: title,
                location: _util.map_location(log, location),
                text: _util.scrub_string($('.product-info').text()),
                html: $('.product-info').html().trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
