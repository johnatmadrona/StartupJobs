var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, n_id) {
    var url = 'http://newton.newtonsoftware.com/career/CareerHome.action?clientId=' + n_id;
    var d = _q.defer();

    log.info({ company: company, n_id: n_id, url: url }, 'Getting jd links');
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
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
            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function scrape_job_description(log, company, title, location, url) {
    log.info({ company: company, title: title, location: location, url: url }, 'Getting jd');

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var description_node = $('#gnewtonJobDescriptionText');
            var jd = {
                url: url,
                company: company,
                title: title,
                location: _util.map_location(log, location),
                text: _util.scrub_string(description_node.text()),
                html: description_node.html().trim()
            };
            console.log(jd);
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
