var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    log.info({ company: company, url: url }, 'Getting jds');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var jds = [];
        $('a[href*="?jobs="]').each(function() {
            jds.push(scrape_job_description(
                log,
                company,
                location,
                $(this).attr('href')
            ));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, location, url) {
    log.debug({ company: company, location: location, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var title_node = $('.heading-text > .entry-title');
        var description_node = $('.body-text');
        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string(title_node.text()),
            location,
            _util.scrub_string(description_node.text()),
            description_node.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
