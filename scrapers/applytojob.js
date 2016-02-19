var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, atj_id) {
    var url = 'http://' + atj_id + '.applytojob.com/apply';
    log.info({ company: company, atj_id: atj_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jd_link_nodes = $('.list-group-item-heading > a');
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
        var $ = _cheerio.load(html);
        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.job-header h1').text()),
            _util.scrub_string($('.job-header *[title="Location"]').text()),
            _util.scrub_string($('.description').text()),
            $('.description').html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
