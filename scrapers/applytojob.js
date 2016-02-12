var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, atj_id) {
    var url = 'http://' + atj_id + '.applytojob.com/apply';
    var d = _q.defer();

    log.info({ company: company, atj_id: atj_id, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var jds = [];
            $('.list-group-item-heading > a').each(function() {
                jds.push(scrapeJobDescription(log, company, $(this).attr('href')));
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
            var jd = _util.create_jd(
                log,
                url,
                company,
                _util.scrub_string($('.job-header h1').text()),
                _util.scrub_string($('.job-header *[title="Location"]').text()),
                _util.scrub_string($('.description').text()),
                $('.description').html().trim()
            );
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
