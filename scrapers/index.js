var jazz = require('./jazz.js');
var jobvite = require('./jobvite.js');

module.exports = {
	scrapers: [
		{
			company: '2nd Watch',
			scrape: function(log) {
				return jazz.scrape(log, '2nd Watch', '2ndwatch', 'http://2ndwatch.com/contact-us/careers/');
			}
		},
		{
			company: 'Animoto',
			scrape: function(log) {
				return jobvite.scrape(log, 'Animoto', 'animoto');
			}
		}
	]
};
