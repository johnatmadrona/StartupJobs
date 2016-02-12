var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jds');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jds = [];
        $('.career a').each(function() {
            var title = _util.scrub_string($(this).find('h3').text());
            var location = _util.scrub_string($(this).find('.location-name').text());
            var jd_url = _node_url.resolve(url, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, title, location, jd_url));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, title, location, url) {
    log.info({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var description = _cheerio.load('<span></span>')('span')
            .append($('.the-position'))
            .append($('.the-requirements'));
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
