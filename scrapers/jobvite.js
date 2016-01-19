var Q = require('q');
var request = require('request');
var cheerio = require('cheerio');
var nodeUrl = require('url');

function scrape(company, jvId) {
    var url = 'http://jobs.jobvite.com/careers/' + jvId + '/jobs?jvi=';
    var d = Q.defer();

    console.log('Getting job links from ' + url);
    request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var jobPagePrefix = '/' + jvId + '/job/';
            var $ = cheerio.load(html);
            var links = $('a[href^="' + jobPagePrefix + '"]');

            console.log(links.length + ' job descriptions found');

            var jds = [];
            links.each(function() {
                var jdUrl = nodeUrl.resolve(url, $(this).attr('href'));
                console.log('Getting job info from ' + jdUrl);
                jds.push(scrapeJobDescription(company, jdUrl));
            });

            d.resolve(Q.all(jds));
        }
    });

    return d.promise;
}

function scrapeJobDescription(company, url) {
    var d = Q.defer();

    console.log('Getting job description from ' + url);
    request(url, function(err, res, html) {
        if (err) {
            d.reject(err);
        } else {
            var $ = cheerio.load(html);
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
