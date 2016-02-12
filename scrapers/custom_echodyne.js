var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    log.info({ company: company, url: url }, 'Getting jds');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jds = [];
        $('.entry-content a[href^="http://echodyne.com/careers/"]').each(function() {
            jds.push(scrape_job_description(log, company, location, $(this).attr('href')));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, location, url) {
    log.info({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var contentNode = $('.entry-content div').eq(1);
        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.entry-title').text()),
            location,
            _util.scrub_string(contentNode.text()),
            contentNode.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
