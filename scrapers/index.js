var _q = require('q');
var _angellist = require('./angellist.js');
var _custom_bocada = require('./custom_bocada.js');
var _custom_echodyne = require('./custom_echodyne.js');
var _custom_envelopvr = require('./custom_envelopvr.js');
var _greenhouse = require('./greenhouse.js');
var _jazz = require('./jazz.js');
var _jobvite = require('./jobvite.js');
var _no_job_page = require('./no_job_page.js');
var _workable = require('./workable');

module.exports = {
	scrapers: [
		{
			company: '2nd Watch',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, '2ndwatch', 'http://2ndwatch.com/contact-us/careers/');
			}
		},
		{
			company: 'Algorithmia',
			scrape: function(log) {
				return _angellist.scrape(log, this.company, 'algorithmia');
			}
		},
		{
			company: 'Animoto',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'animoto');
			}
		},
		{
			company: 'Apptio',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'apptio');
			}
		},
		{
			company: 'Area360',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'http://www.area360.com/#contact-us',
					'Seattle, WA',
					'Chris Smith',
					'chris.smith@area360.com'
				);
			}
		},
		{
			company: 'Bizible',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'bizible1', 'http://www.bizible.com/jobs');
			}
		},
		{
			company: 'Bocada',
			scrape: function(log) {
				return _custom_bocada.scrape(log, this.company, 'Kirkland, WA', 'http://bocada.com/careers/');
			}
		},
		{
			company: 'Boomerang Commerce',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'boomerangcommerce');
			}
		},
		{
			company: 'Booster Fuels',
			scrape: function(log) {
				return _workable.scrape(log, this.company, 'boosterfuels');
			}
		},
		{
			company: 'Branch Metrics',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'branchmetrics');
			}
		},
		{
			company: 'Cape Productions',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'cape');
			}
		},
		{
			company: 'Cedexis',
			scrape: function(log) {
				return _workable.scrape(log, this.company, 'cedexis');
			}
		},
		{
			company: 'Cheezburger',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'cheezburger', 'http://cheezburger.theresumator.com/');
			}
		},
		{
			company: 'Context Relevant',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'contextrelevant');
			}
		},
		{
			company: 'Dato',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'dato');
			}
		},
		{
			company: 'Echodyne',
			scrape: function(log) {
				return _custom_echodyne.scrape(log, this.company, 'Bellevue, WA', 'http://echodyne.com/careers/');
			}
		},
		{
			company: 'Envelop VR',
			scrape: function(log) {
				return _custom_envelopvr.scrape(log, this.company, 'https://www.envelopvr.com/careers');
			}
		},
		{
			company: 'Eventbase',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Evocalize',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'ExtraHop',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'HelloTech',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Highspot',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Icebrg',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Igneous Systems',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Impinj',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Indochino',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Intrepid Learning',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'iSpot.tv',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Jama Software',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Jintronix',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Jobaline',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Jova',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Julep',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'LUMO Bodytech',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Matcherino',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'MaxPoint',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Mixpo',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Mobilewalla',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Opal Labs',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Peach',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'New Company',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Placed',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'PlayFab',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Poppy Care',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Pro.com',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Qumulo',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Redfin',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Rover.com',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Seeq',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Shippable',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Skytap',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Smartsheet',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'SNUPI Technologies',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Spare5',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'WildTangent',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		},
		{
			company: 'Wonder Workshop',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		}

		/*
		,
		{
			company: 'New Company',
			scrape: function(log) {
				return _q.reject(new Error('Not yet implemented'));
			}
		}
		*/
	]
};
