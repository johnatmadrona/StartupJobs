var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    var d = _q.defer();

    var parsed_url = _node_url.parse(url);
    if (parsed_url.hostname !== 'jobs.jobvite.com' &&
        parsed_url.hostname !== 'hire.jobvite.com') {
        throw new Error('Invalid domain for jobvite scraping: ' + parsed_url.hostname);
    }

    log.info({ company: company, url: url }, 'Getting jd links');
    _request({ url: url, followRedirect: true }, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else if (url !== res.request.uri.href) {
            log.error({ original_url: url, redirected_url: res.request.uri.href }, 'URL redirected');
            d.reject(new Error('Request redirected from ' + url + ' to ' + res.request.uri.href));
        } else {
            d.resolve(_q.all(parse_listing_and_scrape_jds(log, company, parsed_url, html)));
        }
    });

    return d.promise;
}

function parse_listing_and_scrape_jds(log, company, parsed_url, html) {
    var $ = _cheerio.load(html);
    var jds = [];
    var links;

    if (parsed_url.hostname === 'jobs.jobvite.com') {
        //var company_id = /\/careers\/(\w+)\//.exec(parsed_url.href)[1];
        links = $('a[href*="/job/"]');
        links.each(function() {
            var jd_url = _node_url.resolve(parsed_url.href, $(this).attr('href'));
            jds.push(scrape_job_description(log, company, jd_url));
        });
    } else if (parsed_url.hostname === 'hire.jobvite.com') {
        var company_id = /[\?&]c=(\w+)/.exec(parsed_url.href)[1];
        links = $('a[onclick^="jvGoToPage(\'Job Description\'"]');
        if (links.length > 0) {
            links.each(function() {
                var jd_url = get_jvGoToPage_url(
                    parsed_url.href.substr(0, parsed_url.href.length - parsed_url.search.length),
                    company_id,
                    $(this).attr('onclick')
                );
                jds.push(scrape_job_description(log, company, jd_url));
            });
        } else {
            links = $('.jobList a');
            links.each(function() {
                var job_id = /[\?&]jvi=([\w,]+)/.exec($(this).attr('href'))[1];
                var jd_url = 'http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&jvresize=&c=' + company_id + '&j=' + escape(job_id);
                jds.push(scrape_job_description(log, company, jd_url));
            });
        }
    } else {
        throw new Error('Invalid domain for jobvite scraping: ' + parsed_url.hostname);
    }

    return jds; 
}

// Parsing roughly matches that used in function jvGoToPage in 
// http://cdn1.jobvite.com/__assets__/legacy/CompanyJobs/careers_8.js.
// Note that in that file, global var jvurlargs is set after including 
// that resource in the consuming html file. In the html, it's set to 
// "?c={company id}".
function get_jvGoToPage_url(base_url, company_id, on_click_text) {
    var params = /jvGoToPage\('(.*?)','(.*?)','(.*?)'\)/.exec(on_click_text);
    var page = params[1];
    var arg = params[2];
    var job_id = params[3];

    var jd_url = base_url + '?c=' + company_id + '&page=' + encodeURIComponent(page);

    if (arg && arg.length) {
        jd_url += '&arg=' + encodeURIComponent(arg);
    }

    if (job_id && job_id.length) {
        jd_url += '&j=' + job_id;
    }

    return jd_url;
}

function scrape_job_description(log, company, url) {
    log.info({ company: company, url: url }, 'Getting jd');

    var parsed_url = _node_url.parse(url);
    if (parsed_url.hostname !== 'jobs.jobvite.com' &&
        parsed_url.hostname !== 'hire.jobvite.com') {
        throw new Error('Invalid domain for jobvite scraping: ' + parsed_url.hostname);
    }

    var d = _q.defer();
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else if (url !== res.request.uri.href) {
            log.error({ original_url: url, redirected_url: res.request.uri.href }, 'URL redirected');
            d.reject(new Error('Request redirected from ' + url + ' to ' + res.request.uri.href));
        } else {
            var $ = _cheerio.load(html);
            var title, location, contentNode;
            if (res.request.uri.host === 'jobs.jobvite.com') {
                title = _util.scrub_string($('.jv-header').text());
                location = _util.scrub_string($('.jv-job-detail-meta').contents()[2].data);
                contentNode = $('.jv-job-detail-description');
            } else if (res.request.uri.host === 'hire.jobvite.com') {
                if (/[\?&]jvresize=/.test(res.request.uri.search)) {
                    title = _util.scrub_string($('.title_jobdesc > h2').text());
                    location = _util.scrub_string($('.title_jobdesc > h3').text().split('|')[1]);
                    contentNode = $('.jobDesc');
                } else {
                    title = _util.scrub_string($('.jvjobheader > h2').text());
                    location = _util.scrub_string($('.jvjobheader > h3').text().split('|')[1]);
                    contentNode = $('.jvdescriptionbody');
                }
            }

            var jd = {
                url: url,
                company: company,
                title: title,
                location: _util.map_location(log, location),
                text: _util.scrub_string(contentNode.text()),
                html: contentNode.html().trim()
            };
            console.log();
            console.log(jd);
            console.log();
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
