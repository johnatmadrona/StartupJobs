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
            $('.careers-page a[href*="/careers/"]').each(function() {
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
            $('.job-post').children().each(function() {
                if (typeof($(this).attr('class')) !== 'undefined' && 
                    $(this).attr('class').includes('apply-button')) {
                    // Stop reading after this point
                    return false;
                }
                description_text += _util.scrub_string($(this).text()) + ' ';
                description_html += $(this).html().trim() + ' ';
            });

            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('.about-headline').text()),
                location: _util.map_location(log, location),
                text: description_text,
                html: description_html
            };
            console.log();
            console.log(jd);
            console.log();
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
