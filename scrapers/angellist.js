var _q = require('q');
var _node_url = require('url');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');
var _proxy_lookup = require('../app_modules/proxy_lookup');

function scrape(log, company, angellist_id) {
    var url = 'https://angel.co/' + angellist_id + '/jobs/';

    return _proxy_lookup.find_proxy(log).then(function(proxy_url) {
        log.info({
            company: company,
            angellist_id: angellist_id,
            url: url,
            proxy_url: proxy_url
        }, 'Getting jd links');
        return _util.request(log, url, { proxy: proxy_url }).then(function(html) {
            var $ = _cheerio.load(html);
            var link_nodes = $('.listing-title > a');
            var jds = [];
            log.info({ company: company, url: url, count: link_nodes.length }, 'Scraping jds');
            link_nodes.each(function() {
                var jd_url = _node_url.resolve(url, $(this).attr('href'));
                jds.push(scrape_job_description(log, company, jd_url, proxy_url));
            });

            return _q.all(jds);
        });
    });
}

function scrape_job_description(log, company, url, proxy) {
    log.debug({ company: company, url: url, proxy: proxy }, 'Getting jd');
    return _util.request(log, url, { proxy: proxy }).then(function(html) {
        var $ = _cheerio.load(html);

        var titleNode = $('.company-summary > h1');

        var locationNode = titleNode.next();
        var locationNodeText = _util.scrub_string(locationNode.text());
        var location = locationNodeText.split('\u00b7')[0].trim();

        var descriptionNode = $('.job-description');

        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string(titleNode.text()),
            location,
            _util.scrub_string(descriptionNode.text()),
            descriptionNode.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
