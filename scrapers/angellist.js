var _q = require('q');
var _node_url = require('url');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, angellist_id) {
    var url = 'https://angel.co/' + angellist_id + '/jobs/';
    log.info({ company: company, angellist_id: angellist_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var listings = $('.listing');
        var jds = [];

        log.info('Found ' + listings.length + ' listings');
        listings.each(function() {
            var titleNode = $(this).find('.title > a');
            var locationNode = $(this).find('.job-data').eq(1);
            jds.push(scrape_job_description(
                log,
                company,
                _util.scrub_string(titleNode.text()),
                _util.scrub_string(locationNode.text()),
                _node_url.resolve(url, titleNode.attr('href'))
            ));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, title, location, url) {
    log.info({ company: company, title: title, location: location, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        return _util.create_jd(
            log,
            url,
            company,
            title,
            location,
            _util.scrub_string($('.product-info').text()),
            $('.product-info').html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
