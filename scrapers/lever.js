var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, lever_id) {
    var url = 'https://jobs.lever.co/' + lever_id + '/';
    log.info({ company: company, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jd_link_nodes = $('a.posting-title');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jds');
        jd_link_nodes.each(function() {
            jds.push(scrape_job_description(log, company, $(this).attr('href')));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.debug({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var jds = [];
        var $ = _cheerio.load(html);

        var content_html = '';
        var content_text = '';
        var content_nodes = $('.content').children().eq(1).find('.section').not('.last-section-apply');
        content_nodes.each(function() {
            content_html += _util.outer_html($(this)) + ' ';
            content_text += _util.scrub_string($(this).text()) + ' ';
        });

        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.posting-headline > h2').text()),
            _util.scrub_string($('.posting-categories').children().eq(0).text()),
            content_text.trim(),
            content_html.trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
