var _q = require('q');
var _node_url = require('url');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, angellist_id) {
    var d = _q.defer();

    var options = {
        url: 'https://angel.co/' + angellist_id + '/jobs/',
        headers: {
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            //'Accept-Encoding': ...'gzip, deflate, sdch',
            'Accept-Language': 'en-US,en;q=0.8,es;q=0.6',
            'Cache-Control': 'max-age=0',
            //'Connection': 'keep-alive',
            //Cookie: ...
            'Host': 'angel.co',
            //If-None-Match: ...
            'Referer': 'https://algorithmia.com/',
            //'Upgrade-Insecure-Requests': 1,
            'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36'
        }
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
    log.info({ company: company, title: title, location: location, url: url }, 'Getting jd');

    var d = _q.defer();

    _request(url, function(err, res, html) {
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
                html: $('.product-info').html()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
