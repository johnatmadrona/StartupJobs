var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, jvId) {
    var url = 'http://jobs.jobvite.com/careers/' + jvId + '/jobs?jvi=';
    var d = _q.defer();

    log.info({ company: company, jvId: jvId, url: url }, 'Getting jd links');
    _request({ url: url, followRedirect: true }, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var jds = [];
            var $ = _cheerio.load(html);
            var links, scrape_function;
            if (res.request.uri.host === 'jobs.jobvite.com') {
                links = $('a[href^="/' + jvId + '/job/"]');
                links.each(function() {
                    var jd_url = _node_url.resolve(url, $(this).attr('href'));
                    jds.push(scrapeJobDescription(log, company, jd_url));
                });
            } else if (res.request.uri.host === 'hire.jobvite.com') {
                var redirected_url = res.request.uri.href;
                log.info({ original_url: url, redirected_url: redirected_url }, 'URL redirected');

                links = $('a[onclick^="jvGoToPage(\'Job Description\'"]');
                links.each(function() {
                    var jd_url = getJobviteGoToPageUrl(redirected_url, $(this).attr('onclick'));
                    jds.push(scrapeJobDescription(log, company, jd_url));
                });
            }

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

// Parsing roughly matches that used in function jvGoToPage in 
// http://cdn1.jobvite.com/__assets__/legacy/CompanyJobs/careers_8.js.
// Note that in that file, global var jvurlargs is set after including 
// that resource in the consuming html file. In the html, it's set to 
// "?c={company id}".
function getJobviteGoToPageUrl(origin_url, on_click_text) {
    var params = /jvGoToPage\('(.*?)','(.*?)','(.*?)'\)/.exec(on_click_text);
    var page = params[1];
    var arg = params[2];
    var job_id = params[3];
    var company_id = /c=(\w+)/.exec(origin_url)[1];

    var jd_url = origin_url;
    var query_offset = origin_url.indexOf('?');
    if (query_offset != -1) {
        jd_url = jd_url.substring(0, query_offset);
    }

    jd_url += '?c=' + company_id + '&page=' + encodeURIComponent(page);

    if (arg && arg.length) {
        jd_url += '&arg=' + encodeURIComponent(arg);
    }

    if (job_id && job_id.length) {
        jd_url += '&j=' + job_id;
    }

    return jd_url;
}

function scrapeJobDescription(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);

            var title, location, contentNode;
            if (res.request.uri.host === 'jobs.jobvite.com') {
                title = _util.scrub_string($('.jv-header').text());
                location = _util.scrub_string($('.jv-job-detail-meta').contents()[2].data);
                contentNode = $('.jv-job-detail-description');
            } else if (res.request.uri.host === 'hire.jobvite.com') {
                title = _util.scrub_string($('.jvjobheader > h2').text());
                location = _util.scrub_string($('.jvjobheader > h3').text().split('|')[1]);
                contentNode = $('.jvdescriptionbody');
            }

            var jd = {
                url: url,
                company: company,
                title: title,
                location: _util.map_location(log, location),
                text: _util.scrub_string(contentNode.text()),
                html: contentNode.html().trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
