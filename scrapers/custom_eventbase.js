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
            $('.views-field-title > .field-content > a').each(function() {
                var jd_url = _node_url.resolve(url, $(this).attr('href'));
	            jds.push(scrapeJobDescription(log, company, location, jd_url));
            });
            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(log, company, location, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jd');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var description_text = '';
            var description_html = '';
            $('div[property="content:encoded"]').children().each(function() {
                if ($(this).text().trim().toLowerCase().startsWith('how to apply')) {
                    // Stop reading after this point
                    return false;
                }
                description_text += _util.scrub_string($(this).text()) + ' ';
                description_html += _util.outer_html($(this)) + ' ';
            });

            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('span[property="dc:title"]').attr('content')),
                location: _util.map_location(log, location),
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
