var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, location, url) {
    log.info({ company: company, url: url }, 'Getting jds');
    return _util.request(log, url).then(function(html) {
        var $ = _cheerio.load(html);

        var jds = [];
        $('*[data-type="page"] *[data-block-type="23"]').each(function() {
            var title = _util.scrub_string($(this).text());
        	var new_jd = _util.create_jd(
                log,
                url,
                company,
                title,
                location,
                _util.scrub_string($(this).next().text()),
                $(this).next().html().trim()
            );
            jds.push(new_jd);
        });

        return _q.all(jds);
    });
}

module.exports = {
    scrape: scrape
};
