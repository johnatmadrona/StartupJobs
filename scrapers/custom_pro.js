var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jds');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jds = [];
        $('.entry-content a[href^="https://jobs.pro.com/?page_id="]').each(function() {
            jds.push(scrape_job_description(log, company, $(this).attr('href')));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var header = $('.entry-title').text();
        var header_divide = header.indexOf(',');
        var title = _util.scrub_string(header.substring(0, header_divide));
        var location = _util.scrub_string(header.substring(header_divide + 1));
        var description = $('.entry-content');

        return _util.create_jd(
            log,
            url,
            company,
            title,
            location,
            _util.scrub_string(description.text()),
            description.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
