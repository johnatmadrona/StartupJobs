var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, jobaline_id) {
    var url = 'http://' + jobaline_id + '.jobaline.com/';
    log.info({ company: company, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var jds = [];
        var $ = _cheerio.load(html);
        $('a.search_job_title').each(function() {
            jds.push(scrape_job_description(log, company, $(this).attr('href')));
        });
        return _q.all(jds);
    });

    return d.promise;
}

function scrape_job_description(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var raw_location = _util.scrub_string($('.info-icons').children().eq(0).text());
        var parsed_location = /([\w ,]+)\d{5}/.exec(raw_location)[1].trim();
        var content_node = $('.job-description-container');
        return _util.create_jd(
            log,
            url,
            company,
            _util.scrub_string($('.job-title').text()),
            parsed_location,
            _util.scrub_string(content_node.text()),
            content_node.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
