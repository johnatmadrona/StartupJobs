var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, jazz_id, public_url) {
    var url = 'http://app.jazz.co/widgets/basic/create/' + jazz_id;
    var d = _q.defer();

    log.info({ company: company, jazz_id: jazz_id, url: url }, 'Getting jd links');
    _request(url, function(err, res, script) {
        if (err) {
            d.reject(err);
        } else {
        	var html_start = script.indexOf('\'<div id="resumator-widget"');
        	if (html_start < 0) {
        		throw new Error('Start of html not found for ' + jazz_id);
        	}
        	html_start++;

        	var html_end =  script.indexOf('\'', html_start);
        	if (html_end < 0) {
        		throw new Error('End of html not found for ' + jazz_id);
        	}

        	var html = script.substring(html_start, html_end);
            var $ = _cheerio.load(html);

            var jds = [];
            $('.resumator-job').each(function() {
            	var raw_loc = _util.scrub_string($(this).find('.resumator-job-info').contents()[1].data);
            	var new_jd = {
	                url: public_url,
	                company: company,
	                title: _util.scrub_string($(this).find('.resumator-job-title').text()),
	                location: _util.map_location(log, raw_loc),
	                text: _util.scrub_string($(this).find('.resumator-job-description-text').text()),
	                html: $(this).find('.resumator-job-description-text').html().trim()
	            };
	            jds.push(new_jd);
            });

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

module.exports = {
    scrape: scrape
};
