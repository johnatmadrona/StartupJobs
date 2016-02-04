var _q = require('q');
var _cheerio = require('cheerio');
var _util = require('./scraper_utils.js');

function scrape(log, company, info_url, location, contact_name, contact_email) {
    var $ = _cheerio.load(
        '<div>For career opportunities at ' + company + 
        ', reach out to ' + contact_name + 
        ' at <a href="mailto:' + contact_email + '?subject=Interested%20in%20a%20career%20at%20' + 
        company.replace(/\s+/g, '%20') + '">' + contact_email + '</a>.</div>'
    );

    // Use Q's .all operation to match behavior of other scrapers
    var result = {
        url: info_url,
        company: company,
        title: 'Opportunistic Role',
        location: _util.map_location(log, location),
        text: _util.scrub_string($.text()),
        html: $.html()
    };
    return _q.all([result]);
}

module.exports = {
    scrape: scrape
};
