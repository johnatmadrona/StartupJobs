var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, w_id) {
    var url = 'https://' + w_id + '.workable.com/';
    log.info({ company: company, w_id: w_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jds = [];
        $('.job a').each(function() {
            var jd_url = _node_url.resolve(url, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, jd_url));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var description_text = '';
        var description_html = '';
        $('.section--text').each(function() {
            description_text += _util.scrub_string($(this).text()) + ' ';
            description_html += _util.outer_html($(this)) + ' ';
        });

        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.section--header > h1').text()),
            _util.scrub_string($('.section--header > .meta').text()),
            description_text.trim(),
            description_html.trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
