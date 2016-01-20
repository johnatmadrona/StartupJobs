var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');

function scrape(log, company, jvId) {
    var url = 'http://jobs.jobvite.com/careers/' + jvId + '/jobs?jvi=';
    var d = _q.defer();

    log.info('Getting job links from ' + url);
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

function scrapeJobDescription(log, company, url) {
    log.info('Getting job description from ' + url);

    var d = _q.defer();

    _request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = _cheerio.load(html);
            $('.jv-job-detail-meta').children()
            d.resolve({
                SourceUri: url,
                Company: company,
                Title: scrubString($('.jv-header').text()),
                Location: scrubString($('.jv-job-detail-meta').contents()[2].data),
                FullTextDescription: scrubString($('.jv-job-detail-description').text()),
                FullHtmlDescription: $('.jv-job-detail-description').html()
            });
        }
    });

    return d.promise;
}

function scrubString(text) {
    text = text.replace('\\\'', '\'');
    text = text.replace('\\n', ' ');
    text = text.replace('\\r', ' ');
    text = text.replace(/\s+/g, ' ');
    return text.trim();
}

module.exports = {
    scrape: scrape
};
