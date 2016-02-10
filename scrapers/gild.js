var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, g_id) {
    var d = _q.defer();

    var url = 'https://people.gild.com/company/' + g_id + '?limit=1000&offset=0';

    log.info({ company: company, g_id: g_id, url: url }, 'Getting jds');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var jds = [];

            if ($('.pagination').length > 0) {
                throw new Error('Pagination not yet implemeneted');
            }

            $('a.view-job').each(function() {
                var jd_url = _node_url.resolve(url, $(this).attr('href'));
	            jds.push(scrape_job_description(log, company, jd_url));
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrape_job_description(log, company, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jd');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var title_node = $('.job-position-header').clone().children().remove().end();

            var location;
            var description_node;
            $('.job-position > dl > dt').each(function() {
                if ($(this).text().trim().toLowerCase() === 'location') {
                    location = _util.scrub_string($(this).next().text());
                }
                if ($(this).text().trim().toLowerCase() === 'job description') {
                    description_node = $(this).next();
                }
            });

            if (typeof(location) === 'undefined') {
                throw new Error('Unexpected format, couldn\'t find location');
            }

            if (typeof(description_node) === 'undefined') {
                throw new Error('Unexpected format, couldn\'t find location');
            }

            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string(title_node.text()),
                location: _util.map_location(log, location),
                text: _util.scrub_string(description_node.text()),
                html: description_node.html().trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
