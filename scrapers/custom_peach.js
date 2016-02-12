var _q = require('q');
var _request = require('request');
var _node_url = require('url');
var _util = require('./scraper_utils.js');

function scrape(log, company, url) {
    var d = _q.defer();

    var jobs_url = _node_url.resolve(url, '/api/jobs/');
    log.info({ company: company, url: url, jobs_url: jobs_url }, 'Getting jd links');
    _request(jobs_url, function(err, res, json) {
        if (err) {
            d.reject(err);
        } else {
            var jobs = JSON.parse(json);
            var jds = [];
            for (var i = 0; i < jobs.length; i++) {
                if (jobs[i].active) {
                    var description = format_description(jobs[i]);
                    var jd = _util.create_jd(
                        log,
                        _node_url.resolve(url, '/jobs/' + jobs[i].id + '/'),
                        company,
                        jobs[i].title,
                        jobs[i].location,
                        description.text,
                        description.html
                    );
                    jds.push(jd);
                }
            }

            d.resolve(_q.all(jds));
        }
    });

    return d.promise;
}

function format_description(job) {
    var text = '';
    var html = '';
    var i;

    text += job.description + '\n';
    html += '<div>' + job.description + '</div>';

    if (job.responsibilities.length > 0) {
        text += '\nResponsibilities:\n';
        html += '<div><h2>Responsibilities</h2><ul>';
        for (i = 0; i < job.responsibilities.length; i++) {
            text += '* ' + job.responsibilities[i] + '\n';
            html += '<li>' + job.responsibilities[i] + '</li>';
        }
        html += '</ul></div>';
    }

    if (job.requirements.length > 0) {
        text += '\nRequirements:\n';
        html += '<div><h2>Requirements</h2><ul>';
        for (i = 0; i < job.requirements.length; i++) {
            text += '* ' + job.requirements[i] + '\n';
            html += '<li>' + job.requirements[i] + '</li>';
        }
        html += '</ul></div>';
    }

    if (job.preferred.length > 0) {
        text += '\nPreferred:\n';
        html += '<div><h2>Preferred</h2><ul>';
        for (i = 0; i < job.preferred.length; i++) {
            text += '* ' + job.preferred[i] + '\n';
            html += '<li>' + job.preferred[i] + '</li>';
        }
        html += '</ul></div>';
    }

    return {
        text: text,
        html: html
    };
}

module.exports = {
    scrape: scrape
};
