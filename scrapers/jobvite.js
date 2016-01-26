var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');

function scrape(log, company, jvId) {
    var url = 'http://jobs.jobvite.com/careers/' + jvId + '/jobs?jvi=';
    var d = _q.defer();

    log.info('Getting jd links from ' + url);
    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var jobPagePrefix = '/' + jvId + '/job/';
            var $ = _cheerio.load(html);
            var links = $('a[href^="' + jobPagePrefix + '"]');

            var jds = [];
            links.each(function() {
                var jdUrl = _node_url.resolve(url, $(this).attr('href'));
                jds.push(scrapeJobDescription(log, company, jdUrl));
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function getHashableText(name) {
    return name.replace(/[\s\.]+/g, '-').toLowerCase();
}

var _city_lookup = require('./city_lookup_map.json');
var _state_lookup = require('./state_lookup_map.json');
var _country_lookup = require('./country_lookup_map.json');
function scrapeJobDescription(log, company, url) {
    log.info('Getting jd from ' + url);

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            var jd = {
                url: url,
                company: company,
                title: scrubString($('.jv-header').text()),
                location: {
                    raw: scrubString($('.jv-job-detail-meta').contents()[2].data)
                },
                text: scrubString($('.jv-job-detail-description').text()),
                html: $('.jv-job-detail-description').html()
            };

            var loc_parts = jd.location.raw.split(',');
            for (var i = 0; i < loc_parts.length; i++) {
                var loc = getHashableText(loc_parts[i]);
                var city, state;
                if (_city_lookup[loc]) {
                    city = _city_lookup[loc];
                    state = _state_lookup[city.state];
                    jd.location.city = city.canonicalName;
                    jd.location.state = state.canonicalName;
                    jd.location.country = _country_lookup[state.country].canonicalName;
                    break;
                } else if (_state_lookup[loc]) {
                    state = _state_lookup[loc];
                    jd.location.state = state.canonicalName;
                    jd.location.country = _country_lookup[state.country].canonicalName;
                    break;
                } else if (_country_lookup[loc]) {
                    jd.location.country = _country_lookup[loc].canonicalName;
                    break;
                }
            }

            // If we couldn't find the location, set the city to the unknown value
            if (typeof(jd.location.country) === 'undefined') {
                log.warn(
                    { location: rawJob.Location, id: key, company: rawJob.Company, title: rawJob.Title },
                    'Location not found in lookup maps'
                );
                jd.location.city = rawJob.Location;
            }

            d.resolve(jd);
        }
    });

    return d.promise;
}

function scrubString(text) {
    text = text.replace('\\\'', '\'');
    text = text.replace('\\"', '"');
    text = text.replace('\\n', ' ');
    text = text.replace('\\r', ' ');
    text = text.replace(/\s+/g, ' ');
    return text.trim();
}

module.exports = {
    scrape: scrape
};
