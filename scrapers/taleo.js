var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, t_id) {
    var url = 'http://chj.tbe.taleo.net/chj04/ats/careers/searchResults.jsp?org=' + t_id + '&cws=4';
    log.info({ company: company, t_id: t_id, url: url }, 'Getting jd links');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var nav_nodes = $('input[title="Previous Page"]');
        if (nav_nodes.length > 0) {
            var match = /(\d+)-(\d+) of (\d+)/.exec(nav_nodes.eq(0).parent().next().text().trim());
            if (match[2] !== match[3]) {
                return _q.reject(new Error('Pagination not yet implemented'));
            }
        }

        var jd_link_nodes = $('a[href*="/requisition.jsp"]');
        var jds = [];

        log.info({ company: company, count: jd_link_nodes.length }, 'Getting jds');
        jd_link_nodes.each(function() {
            var jd_url = $(this).attr('href').replace(/;jsessionid=[\dA-Z]+\?/, '?');
            jds.push(scrape_job_description(log, company, jd_url));
        });

        return _q.all(jds);
    });
}

function scrape_job_description(log, company, url) {
    log.debug({ company: company, url: url }, 'Getting jd');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        // There may by multiple h1 elements, but the first one is 
        // the one we're looking for
        var title = _util.scrub_string($('#taleoContent > table h1').text());
        var location, description_node;
        $('#taleoContent > table > tr').each(function() {
            var text = _util.scrub_string($(this).text());
            if (text.startsWith('Location:')) {
                location = _util.scrub_string($(this).children().eq(1).text());
            }
            if (text === 'Description') {
                description_node = $(this).next();
            }
        });
        if (typeof(location) === 'undefined') {
            return _q.reject(new Error('Unexpected format, couldn\'t find location'));
        }
        if (typeof(description_node) === 'undefined') {
            return _q.reject(new Error('Unexpected format, couldn\'t find description'));
        }

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
