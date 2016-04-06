var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jd_link_nodes = $('.views-field-title > .field-content > a');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jd links');
        jd_link_nodes.each(function() {
            var jd_url = _node_url.resolve(url, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, jd_url));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.debug({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var title_node = $('.content-wrapper > h1');
        var location_node = $('.content-wrapper > .secondary-header > .secondary-item').eq(1);
        var description_node = $('.content-wrapper .body');

        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string(title_node.text()),
            _util.scrub_string(location_node.text()),
            _util.scrub_string(description_node.text()),
            description_node.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
