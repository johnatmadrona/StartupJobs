var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, n_id) {
    var url = 'http://newton.newtonsoftware.com/career/CareerHome.action?clientId=' + n_id;
    log.info({ company: company, n_id: n_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var jds = [];
        $('.gnewtonCareerGroupRowClass').each(function() {
            var jd_link_node = $(this).find('.gnewtonCareerGroupJobTitleClass > a');
            var location_node = $(this).find('.gnewtonCareerGroupJobDescriptionClass');
            jds.push(scrape_job_description(
                log,
                company,
                _util.scrub_string(jd_link_node.text()),
                _util.scrub_string(location_node.text()),
                jd_link_node.attr('href')
            ));
        });
        return _q.all(jds);
    });
}

function scrape_job_description(log, company, title, location, url) {
    log.info({ company: company, title: title, location: location, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);
        var description_node = $('#gnewtonJobDescriptionText');
        return _util.create_jd(
            log,
            url,
            company,
            title,
            location,
            _util.scrub_string(description_node.text()),
            description_node.html().trim()
        );
    });
}

module.exports = {
    scrape: scrape
};
