var _q = require('q');
var _angellist = require('./angellist.js');
var _applytojob = require('./applytojob.js');
var _custom_bocada = require('./custom_bocada.js');
var _custom_echodyne = require('./custom_echodyne.js');
var _custom_envelopvr = require('./custom_envelopvr.js');
var _custom_eventbase = require('./custom_eventbase.js');
var _custom_highspot  = require('./custom_highspot.js');
var _custom_peach = require('./custom_peach.js');
var _custom_pro = require('./custom_pro.js');
var _custom_spare5 = require('./custom_spare5.js');
var _gild = require('./gild.js');
var _greenhouse = require('./greenhouse.js');
var _jazz = require('./jazz.js');
var _jobaline = require('./jobaline.js');
var _jobvite = require('./jobvite.js');
var _lever = require('./lever.js');
var _newton = require('./newton.js');
var _no_job_page = require('./no_job_page.js');
var _recruiterbox = require('./recruiterbox.js');
var _taleo = require('./taleo.js');
var _workable = require('./workable');

module.exports = {
	scrapers: [
		{
			company: '2nd Watch',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, '2ndwatch', 'http://2ndwatch.com/about-us/careers/');
			}
		},
		{
			company: 'Algorithmia',
			scrape: function(log) {
				return _angellist.scrape(log, this.company, 'algorithmia');
			}
		},
		{
			company: 'Amperity',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'https://amperity.com/',
					'Seattle, WA',
					'Kabir Shahani',
					'kabir@amperity.com'
				);
			}
		},
		{
			company: 'Animoto',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'http://jobs.jobvite.com/careers/animoto/');
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
				return _jobvite.scrape(log, this.company, 'http://jobs.jobvite.com/careers/boomerangcommerce/');
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
				return _no_job_page.scrape(
					log,
					this.company,
					'http://www.cheezburger.com/',
					'Seattle, WA',
					'the team',
					'contactus@cheezburger.com'
				);
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
				return _jobvite.scrape(log, this.company, 'http://jobs.jobvite.com/dato/');
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
				return _custom_eventbase.scrape(log, this.company, 'Vancouver, BC', 'https://www.eventbase.com/careers');
			}
		},
		{
			company: 'Evocalize',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'http://evocalize.com/about/jobs',
					'Seattle, WA',
					'the team',
					'jobs@evocalize.com'
				);
			}
		},
		{
			company: 'ExtraHop',
			scrape: function(log) {
		 		return _jobvite.scrape(log, this.company, 'https://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qAYaVfwn&jvresize=');
			}
		},
		{
			company: 'HelloTech',
			scrape: function(log) {
		 		return _angellist.scrape(log, this.company, 'hellotech');
			}
		},
		{
			company: 'Highspot',
			scrape: function(log) {
				return _custom_highspot.scrape(log, this.company, 'Seattle, WA', 'https://www.highspot.com/careers/');
			}
		},
		{
			company: 'Icebrg',
			scrape: function(log) {
				return _angellist.scrape(log, this.company, 'icebrg-io');
			}
		},
		{
			company: 'Igneous Systems',
			scrape: function(log) {
		 		return _jobvite.scrape(log, this.company, 'http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qpO9Vfw1&jvresize=');
			}
		},
		{
			company: 'Impinj',
			scrape: function(log) {
		 		return _jobvite.scrape(log, this.company, 'http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qPD9Vfwg&jvresize=');
			}
		},
		{
			company: 'Indochino',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qWn9Vfw7&jvresize=');
			}
		},
		{
			company: 'iSpot.tv',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qrI9VfwX&jvresize=');
			}
		},
		{
			company: 'Jama Software',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'jamasoftware');
			}
		},
		{
			company: 'Jintronix',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'http://www.jintronix.com/team/',
					'Montreal, QC, Canada',
					'the team',
					'careers@jintronix.com'
				);
			}
		},
		{
			company: 'Jobaline',
			scrape: function(log) {
				return _jobaline.scrape(log, this.company, 'jobalineinc');
			}
		},
		{
			company: 'Jova',
			scrape: function(log) {
				return _angellist.scrape(log, this.company, 'jova');
			}
		},
		{
			company: 'Julep',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'julepbeauty', 'http://www.julep.com/careers.html');
			}
		},
		{
			company: 'LUMO Bodytech',
			scrape: function(log) {
				return _lever.scrape(log, this.company, 'lumo');
			}
		},
		{
			company: 'Matcherino',
			scrape: function(log) {
				return _angellist.scrape(log, this.company, 'matcherino');
			}
		},
		{
			company: 'MaxPoint',
			scrape: function(log) {
				return _newton.scrape(log, this.company, '8afc05ca36a0fff80136a2d219b93475');
			}
		},
		{
			company: 'Mixpo',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'https://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qc3aVfw4&jvresize=');
			}
		},
		{
			company: 'Mobilewalla',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'https://www.mobilewalla.com/contact%20us.html',
					'Seattle, WA',
					'the team',
					'contact@mobilewalla.com'
				);
			}
		},
		{
			company: 'Opal',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'opal', 'http://jobs.workwithopal.com/');
			}
		},
		{
			company: 'Peach',
			scrape: function(log) {
				return _custom_peach.scrape(log, this.company, 'https://www.peachd.com/jobs/');
			}
		},
		{
			company: 'Pixvana',
			scrape: function(log) {
				return _recruiterbox.scrape(log, this.company, '39683', 'http://www.pixvana.com/jobs/');
			}
		},
		{
			company: 'Placed',
			scrape: function(log) {
				return _applytojob.scrape(log, this.company, 'placed');
			}
		},
		{
			company: 'PlayFab',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'playfab');
			}
		},
		{
			company: 'Poppy Care',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'http://www.meetpoppy.com/',
					'Seattle, WA',
					'Avni',
					'avni@poppyteam.com'
				);
			}
		},
		{
			company: 'Pro.com',
			scrape: function(log) {
				return _custom_pro.scrape(log, this.company, 'https://jobs.pro.com/');
			}
		},
		{
			company: 'Qumulo',
			scrape: function(log) {
				return _greenhouse.scrape(log, this.company, 'qumulo');
			}
		},
		{
			company: 'Redfin',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'https://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qf49Vfw7&jvresize=');
			}
		},
		{
			company: 'Rover.com',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'rover', 'http://jobs.rover.com/');
			}
		},
		{
			company: 'Seeq',
			scrape: function(log) {
				return _recruiterbox.scrape(log, this.company, '9417', 'https://www.seeq.com/about/careers');
			}
		},
		{
			company: 'Shippable',
			scrape: function(log) {
				return _no_job_page.scrape(
					log,
					this.company,
					'https://app.shippable.com/company.html',
					'Seattle, WA',
					'the team',
					'resumes@shippable.com'
				);
			}
		},
		{
			company: 'Skytap',
			scrape: function(log) {
				return _jobvite.scrape(log, this.company, 'http://jobs.jobvite.com/skytap/');
			}
		},
		{
			company: 'Smartsheet',
			scrape: function(log) {
				return _gild.scrape(log, this.company, 'smartsheet.com');
			}
		},
		{
			company: 'Spare5',
			scrape: function(log) {
				return _custom_spare5.scrape(log, this.company, 'Seattle, WA', 'https://spare5.com/hiring/');
			}
		},
		{
			company: 'WildTangent',
			scrape: function(log) {
				return _taleo.scrape(log, this.company, 'WILDTANGENT');
			}
		},
		{
			company: 'Wonder Workshop',
			scrape: function(log) {
				return _jazz.scrape(log, this.company, 'Playi', 'https://www.makewonder.com/careers');
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
