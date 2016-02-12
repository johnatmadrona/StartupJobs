var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, t_id) {
    var d = _q.defer();
    var url = 'http://chj.tbe.taleo.net/chj04/ats/careers/searchResults.jsp?org=' + t_id + '&cws=4';
    log.info({ company: company, t_id: t_id, url: url }, 'Getting jds');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
            return;
        }

        var $ = _cheerio.load(html);

        var nav_nodes = $('input[title="Previous Page"]');
        if (nav_nodes.length > 0) {
            var match = /(\d+)-(\d+) of (\d+)/.exec(nav_nodes.eq(0).parent().next().text().trim());
            if (match[2] !== match[3]) {
                d.reject(new Error('Pagination not yet implemented'));
                return;
            }
        }

        var jds = [];
        $('a[href*="/requisition.jsp"]').each(function() {
            var jd_url = $(this).attr('href').replace(/;jsessionid=[\dA-Z]+\?/, '?');
            jds.push(scrapeJobDescription(log, company, jd_url));
        });

        d.resolve(_q.all(jds));
    });

    return d.promise;
}

function scrapeJobDescription(log, company, url) {
    var d = _q.defer();

    log.info({ company: company, url: url }, 'Getting jd');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
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
                throw new Error('Unexpected format, couldn\'t find location');
            }
            if (typeof(description_node) === 'undefined') {
                throw new Error('Unexpected format, couldn\'t find description');
            }

            var jd = _util.create_jd(
                log,
                url,
                company,
                title,
                location,
                _util.scrub_string(description_node.text()),
                description_node.html().trim()
            );
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
