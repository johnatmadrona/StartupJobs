var _q = require('q');
var _request = require('request');

// Proxy from a list of known, tested proxies at http://proxyrox.com/top-proxies.
function find_known_proxy(log) {
    return _q('http://207.253.234.227:8080');
}

// Using proxy listing API from http://gimmeproxy.com/. 
function find_random_proxy(log, max_attempts) {
    if (max_attempts < 1) {
        return _q.reject(new Error('Failed to find a working proxy within attempt limit'));
    }

    var url = 'http://gimmeproxy.com/api/getProxy';
    return _q.nfcall(_request, { url: url }).spread(function(res, payload) {
        if (res.statusCode != 200) {
            log.error(
				{
					url: url,
					status_code: res.statusCode,
					status_message: res.statusMessage,
					headers: res.headers
				},
            	'Proxy request service responded with an unexpected status code'
            );
            return _q.reject(new Error('Proxy request service responded with status code ' + res.statusCode));
		}

        var proxy_info = JSON.parse(payload);
        return test_proxy(log, proxy_info.curl).then(function(useable) {
            if (useable) {
                return proxy_info.curl;
            } else {
                return find_proxy(log, max_attempts - 1);
            }
        });
    }, function(err) {
    	log.error({ url: url, err: err }, 'HTTP request for proxy info failed');
    	throw err;	// Rethrow to handler further down chain
    });
}

function test_proxy(log, proxy_url) {
    var request_options = {
        url: 'https://www.google.com',
        proxy: proxy_url,
        headers: {
            'User-Agent': 'startup-jobs'
        }
    };

    return _q.nfcall(_request, request_options).spread(function(res) {
        return res.statusCode === 200;
    }, function(err) {
        return false;
    });
}

module.exports = {
    find_known_proxy: find_known_proxy,
	find_random_proxy: find_random_proxy
};