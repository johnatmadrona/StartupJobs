var _q = require('q');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, g_id) {
    var url = 'https://people.gild.com/company/' + g_id + '?limit=1000&offset=0';
    log.info({ company: company, g_id: g_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        if ($('.pagination').length > 0) {
            return _q.reject(new Error('Pagination not yet implemeneted'));
        }

        var jd_link_nodes = $('a.view-job');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jds');
        jd_link_nodes.each(function() {
            var jd_url = _node_url.resolve(url, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, jd_url));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.debug({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var title_node = $('.job-position-header').clone().children().remove().end();

        var location;
        var description_node;
        $('.job-position > dl > dt').each(function() {
            if ($(this).text().trim().toLowerCase() === 'location') {
                location = _util.scrub_string($(this).next().text());
            }
            if ($(this).text().trim().toLowerCase() === 'job description') {
                description_node = $(this).next();
            }
        });

        if (typeof(location) === 'undefined') {
            return _q.reject(new Error('Unexpected format, couldn\'t find location'));
        } else if (typeof(description_node) === 'undefined') {
            return _q.reject(new Error('Unexpected format, couldn\'t find location'));
        }

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
