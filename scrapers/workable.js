var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, w_id) {
    var url = 'https://' + w_id + '.workable.com/';
    var d = _q.defer();

    log.info({ company: company, w_id: w_id, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var links = $('.job a');
            var jds = [];
            links.each(function() {
                var jd_url = _node_url.resolve(url, $(this).attr('href'));
                jds.push(scrapeJobDescription(log, company, jd_url));
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
            var loc = _util.scrub_string($('.section--header > .meta').text());

            var description_text = '';
            var description_html = '';
            $('.section--text').each(function() {
                description_text += _util.scrub_string($(this).text()) + ' ';
                description_html += $(this).html() + ' ';
            });

            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('.section--header > h1').text()),
                location: _util.map_location(log, loc),
                text: description_text,
                html: description_html
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
