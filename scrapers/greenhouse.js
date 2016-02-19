var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, gh_id) {
    var url = 'https://boards.greenhouse.io/embed/job_board?for=' + gh_id;
    log.info({ company: company, gh_id: gh_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jd_link_nodes = $('.opening > a');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jds');
        jd_link_nodes.each(function() {
            var jd_home_url = $(this).attr('href');
            var jd_id = /gh_jid=(\d+)/.exec(jd_home_url)[1];
            jds.push(scrape_job_description(log, company, jd_home_url, gh_id, jd_id));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, jd_home_url, gh_id, jd_id) {
    var url = 'https://boards.greenhouse.io/embed/job_app?for=' + gh_id + '&token=' + jd_id;
    log.debug(
        { company: company, jd_home_url: jd_home_url, gh_id: gh_id, jd_id: jd_id, url: url },
        'Getting jd'
    );
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        return _util.create_jd(
            log,
            jd_home_url,
            company,
            _util.scrub_string($('.app-title').text()),
            _util.scrub_string($('.location').text()),
            _util.scrub_string($('#content').text()),
            $('#content').html()
        );
    });
}

module.exports = {
    scrape: scrape
};
