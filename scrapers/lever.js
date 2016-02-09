var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, lever_id) {
    var d = _q.defer();

    var url = 'https://jobs.lever.co/' + lever_id + '/';

    log.info({ company: company, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var jds = [];
            var $ = _cheerio.load(html);
            $('a.posting-title').each(function() {
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
            var location = _util.scrub_string($('.posting-categories').children().eq(0).text());

            var content_html = '';
            var content_text = '';
            var content_nodes = $('.content').children().eq(1).find('.section').not('.last-section-apply');
            content_nodes.each(function() {
                content_html += _util.outer_html($(this)) + ' ';
                content_text += _util.scrub_string($(this).text()) + ' ';
            });

            var jd = {
                url: url,
                company: company,
                title: _util.scrub_string($('.posting-headline > h2').text()),
                location: _util.map_location(log, location),
                text: content_text,
                html: content_html.trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
