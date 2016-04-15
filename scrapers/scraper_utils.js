var _q = require('q');
var _cheerio = require('cheerio');
var _request = require('request');
var _city_lookup = require('./lookup_map_city.json');
var _state_lookup = require('./lookup_map_state.json');
var _country_lookup = require('./lookup_map_country.json');
var _job_category_lookup = require('./lookup_map_job_category.json');

function request(log, url, options) {
	var request_options = {
		url: url,
		headers: {
			'User-Agent': 'startup-jobs'
		}
	};

	if (typeof(options) !== 'undefined' && typeof(options.proxy) === 'string') {
		log.debug({ proxy: options.proxy, url: url }, 'Tunneling request through proxy');
		request_options.proxy = options.proxy;
	}

    return _q.nfcall(_request, request_options).spread(function(res, payload) {
        if (res.statusCode != 200) {
            log.error(
				{
					url: url,
					status_code: res.statusCode,
					status_message: res.statusMessage,
					headers: res.headers
				},
            	'Server responded with an unexpected status code'
            );
            return _q.reject(new Error('Unexpected response from server with status code ' + res.statusCode));
		} else if (!are_urls_equivalent(url, res.request.uri.href, true)) {
			if (typeof(options) !== 'undefined' && 
				typeof(options.allow_redirect) !== 'undefined' && 
				options.allow_redirect === true) {
				log.warn({ original_url: url, redirected_url: res.request.uri.href }, 'URL redirected');
			} else {
	            log.error(
	            	{ original_url: url, redirected_url: res.request.uri.href },
	            	'URL redirected and redirects not allowed'
	            );
	            return _q.reject(new Error('Request redirected from ' + url + ' to ' + res.request.uri.href));
	        }
        }

        return payload;
    }, function(err) {
    	log.error({ url: url, err: err }, 'HTTP request failed');
    	throw err;	// Rethrow to handler further down chain
    });
}

function are_urls_equivalent(first, second, ignore_protocol) {
    var f = first;
    var s = second;
    if (typeof(ignore_protocol) !== 'undefined' && ignore_protocol) {
        f_start = first.indexOf('//');
        f = f.substring(f_start);
        s_start = second.indexOf('//');
        s = s.substring(s_start);
    }
    return f === s;
}

function create_jd(
	log,
	url,
	company,
	title,
	location_text,
	text_description,
	html_description
	) {

	if (typeof(url) !== 'string' || url === null || url.length < 1 ||
		typeof(company) !== 'string' || company === null || company.length < 1 ||
		typeof(title) !== 'string' || title === null || title.length < 1 ||
		typeof(location_text) !== 'string' || location_text === null ||
		typeof(text_description) !== 'string' || text_description === null ||
		typeof(html_description) !== 'string' || html_description === null
		) {

		log.error({
			url: url,
			company: company,
			title: title,
			location_text: location_text,
			text_description: text_description,
			html_description: html_description
		}, 'Error creating jd - invalid params');
		throw new Error('All parameters must be provided and of type "string"');
	}

    return {
        url: url,
        company: company,
        title: title,
        category: get_job_category(title),
        location: map_location(log, location_text),
        text_description: text_description,
        html_description: html_description
    };
}

function get_hashable_text(name) {
    return name.replace(/[\s\.]+/g, '-').toLowerCase();
}

function map_location(log, raw_location) {
	var mapped = {
		raw: raw_location,
        region: 'Other'
	};

	var loc_parts = raw_location.split(',');
	for (var i = 0; i < loc_parts.length; i++) {
		var entity = loc_parts[i];
		if (entity.toLowerCase().startsWith('greater ')) {
			entity = entity.substring(8);
		}

		if (entity.toLowerCase().endsWith(' area')) {
			entity = entity.substring(0, entity.length - 5);
		}

	    var key = get_hashable_text(entity);
	    var city, state;
	    if (_city_lookup[key]) {
	        city = _city_lookup[key];
	        state = _state_lookup[city.state];
	        mapped.city = city.canonicalName;
	        mapped.state = state.canonicalName;
	        mapped.region = state.region;
	        mapped.country = _country_lookup[state.country].canonicalName;
	        break;
	    } else if (_state_lookup[key]) {
	        state = _state_lookup[key];
	        mapped.state = state.canonicalName;
	        mapped.region = state.region;
	        mapped.country = _country_lookup[state.country].canonicalName;
	        break;
	    } else if (_country_lookup[key]) {
	        mapped.country = _country_lookup[key].canonicalName;
	        break;
	    }
	}

	// If we couldn't find the location, set the city to the unknown value
	if (typeof(mapped.country) === 'undefined') {
	    log.debug({ raw_location: raw_location }, 'Location not found in lookup maps');
	    mapped.city = raw_location;
	}

	return mapped;
}

// For now, this is a simple heuristic that runs on the job title. 
// Could probably do better by creating a model based on job description.
function get_job_category(job_title) {
	for (var key in _job_category_lookup) {
		if (contains_one_of(job_title, _job_category_lookup[key])) {
			return key;
		}
	}
	return "other";
}

function contains_one_of(text, terms) {
	var lower_case = text.toLowerCase();
	for (var i = 0; i < terms.length; i++) {
		if (lower_case.indexOf(terms[i].toLowerCase()) != -1) {
			return true;
		}
	}
	return false;
}

function outer_html(cheerio_node) {
	return _cheerio.load('<span></span>')('span').append(cheerio_node.clone()).html();
}

function scrub_string(text) {
    var result = text.replace('\\\'', '\'');
    result = result.replace('\\"', '"');
    result = result.replace('\\n', ' ');
    result = result.replace('\\r', ' ');
    result = result.replace(/\s+/g, ' ');
    return result.trim();
}

module.exports = {
	create_jd: create_jd,
	get_job_category: get_job_category,
	outer_html: outer_html,
	request: request,
	scrub_string: scrub_string
};
