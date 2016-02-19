var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    log.info({ company: company, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jd_link_nodes = $('.careers-page a[href*="/careers/"]');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jds');
        jd_link_nodes.each(function() {
            var jd_url = _node_url.resolve(url, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, location, jd_url));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, location, url) {
    log.debug({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var description_text = '';
        var description_html = '';
        $('.job-post').children().each(function() {
            if (typeof($(this).attr('class')) !== 'undefined' && 
                $(this).attr('class').includes('apply-button')) {
                // Stop reading after this point
                return false;
            }
            description_text += _util.scrub_string($(this).text()) + ' ';
            description_html += _util.outer_html($(this)) + ' ';
        });

        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.about-headline').text()),
            location,
            description_text,
            description_html
        );
    });
}

module.exports = {
    scrape: scrape
};
