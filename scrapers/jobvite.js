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
        } else if (!are_urls_equal(url, res.request.uri.href, true)) {
            log.error({ original_url: url, redirected_url: res.request.uri.href }, 'URL redirected');
            d.reject(new Error('Request redirected from ' + url + ' to ' + res.request.uri.href));
        } else {
            d.resolve(parse_listing_and_scrape_jds(log, company, parsed_url, html));
        }
    });

    return d.promise;
}

function are_urls_equal(first, second, ignore_protocol) {
    var f = first;
    var s = second;
    if (typeof(ignore_protocol) !== 'undefined' && ignore_protocol) {
        f_start = first.indexOf('//');
        f = f.substring(f_start);
        s_start = second.indexOf('//');
        s = s.substring(s_start);
    }
    return f === s;
}

function parse_listing_and_scrape_jds(log, company, parsed_url, html) {
    var jds = [];

    if (parsed_url.hostname === 'jobs.jobvite.com') {
        return get_jobs_style_links(parsed_url, html).then(function(urls) {
            for (var i = 0; i < urls.length; i++) {
                jds.push(scrape_job_description(log, company, urls[i]));
            }
            return _q.all(jds);
        });
    } else if (parsed_url.hostname === 'hire.jobvite.com') {
        var $ = _cheerio.load(html);
        var links;

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
            links = $('.jobList a[href*="jvi="]');
            if (links.length < 1) {
                links = $('.joblist a[href*="jvi="]');
            }
            links.each(function() {
                var job_id = /[\?&]jvi=([\w,]+)/.exec($(this).attr('href'))[1];
                var jd_url = 'https://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&jvresize=&c=' + company_id + '&j=' + escape(job_id);
                jds.push(scrape_job_description(log, company, jd_url));
            });
        }
        return _q.all(jds); 
    } else {
        throw new Error('Invalid domain for jobvite scraping: ' + parsed_url.hostname);
    }
}

function get_jobs_style_links(parsed_url, base_html) {
    var promises = [];
    var links = [];

    var $base = _cheerio.load(base_html);

    $base('a[href*="/job/"]').each(function() {
        links.push(_node_url.resolve(parsed_url.href, $base(this).attr('href')));
    });

    $base('a[href*="/search?"]').each(function() {
        var d = _q.defer();
        var url = _node_url.resolve(parsed_url.href, $base(this).attr('href'));
        _request(url, function(err, res, html) {
            if (err) {
                d.reject(err);
            } else {
                var $ = _cheerio.load(html);
                var match = /(\d+)-(\d+) of (\d+)/.exec($('.jv-pagination-text').text().trim());
                if (match[2] !== match[3]) {
                    throw new Error('Pagination not yet implemented');
                }
                $('a[href*="/job/"]').each(function() {
                    links.push(_node_url.resolve(parsed_url.href, $(this).attr('href')));
                });
                d.resolve();
            }
        });
        promises.push(d.promise);
    });

    return _q.all(promises).then(function() {
        // Filter for unique urls
        return links.sort().reduce(function(list, value) {
            if (list.length < 1 || list[list.length - 1] !== value) {
                list.push(value);
            }
            return list;
        }, []);
    });
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
        } else if (!are_urls_equal(url, res.request.uri.href, true)) {
            log.error({ original_url: url, redirected_url: res.request.uri.href }, 'URL redirected');
            d.reject(new Error('Request redirected from ' + url + ' to ' + res.request.uri.href));
        } else {
            var $ = _cheerio.load(html);
            var title, location, content_node;
            if (res.request.uri.host === 'jobs.jobvite.com') {
                title = _util.scrub_string($('.jv-header').text());
                location = _util.scrub_string($('.jv-job-detail-meta').contents()[2].data);
                content_node = $('.jv-job-detail-description');
            } else if (res.request.uri.host === 'hire.jobvite.com') {
                var location_node;
                if ($('.title_jobdesc').length > 0) {
                    title = _util.scrub_string($('.title_jobdesc > h2').text());
                    location_node = $('.title_jobdesc > h3');
                    content_node = $('.jobDesc');
                } else if ($('.jvjobheader').length > 0) {
                    title = _util.scrub_string($('.jvjobheader > h2').text());
                    location_node = $('.jvjobheader > h3');
                    content_node = $('.jvdescriptionbody');
                } else if ($('.jvheader').length > 0) {
                    // This may be custom code for Indochino
                    title = _util.scrub_string($('.jvheader').text());
                    location_node = $('.jvheader').next();
                    content_node = _cheerio.load('<span></span>')('span');
                    $('.jvheader').parent().children('p, ul, h2').each(function() {
                        content_node.append($(this).clone());
                    });
                } else if ($('.jvcontent').length > 0) {
                    // This may be custom code for Redfin
                    var title_node = $('.jvcontent h1');
                    title = _util.scrub_string(title_node.text());
                    location_node = title_node.parent().children('h2');
                    content_node = $('.jobDesc');
                } else {
                    throw new Error('Unexpected format');
                }
                location = _util.scrub_string(location_node.text().split('|')[1]);
            }

            var jd = {
                url: url,
                company: company,
                title: title,
                location: _util.map_location(log, location),
                text: _util.scrub_string(content_node.text()),
                html: content_node.html().trim()
            };
            d.resolve(jd);
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
