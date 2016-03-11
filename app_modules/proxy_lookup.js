var _q = require('q');
var _request = require('request');
var _cheerio = require('cheerio');
var _node_url = require('url');

function find_proxy(log) {
    // The following were found via http://proxyrox.com/top-proxies on 
    // 2016-03-09. The site disallows scraping (as done by find_proxy_internal), 
    // so getting an env variable is a temp work-around until a decision 
    // is made to purchase data.

    // 'http://74.81.130.106:34002'
    // 'http://47.88.104.164:3128'
    // 'http://74.82.168.153:1080'
    // 'http://104.45.152.53:3128'
    // 'http://104.209.182.251:3128'
    // 'http://52.2.54.139:3128'
    // 'http://96.36.19.66:3128'
    // 'http://146.20.68.224:3128'
    // 'http://52.11.232.88:3128'
    // 'http://64.207.93.203:34002'

    // IMPORTANT: Env variable HTTP_PROXY is used by 'request' module, 
    // hencet the name ACTIVE_HTTP_PROXY
    return _q(process.env.ACTIVE_HTTP_PROXY);
}

function find_proxy_internal(log) {
    var url = 'http://proxyrox.com/top-proxies';
    return _q.nfcall(_request, { url: url }).spread(function(res, html) {
        if (res.statusCode != 200) {
            log.error(
                {
                    url: url,
                    status_code: res.statusCode,
                    status_message: res.statusMessage,
                    headers: res.headers
                },
                'HTTP request for proxy list produced an unexpected status code'
            );
            return _q.reject(new Error('HTTP request for proxy list produced status code ' + res.statusCode));
        }

        var $ = _cheerio.load(html);
        var proxy_info_urls = [];
        $('a[href^="/proxy/"]').each(function() {
            proxy_info_urls.push(_node_url.resolve(url, $(this).attr('href')));
        });

        return get_first_working_proxy(log, proxy_info_urls, 0);
    }, function(err) {
        log.error({ url: url, err: err }, 'HTTP request for proxy list failed');
        throw err;  // Rethrow to handler further down chain
    });
}

function get_first_working_proxy(log, proxy_info_urls, index) {
    if (index >= proxy_info_urls.length) {
        return _q.reject(new Error('Unable to find working proxy'));
    }

    return get_proxy_info(log, proxy_info_urls[index]).then(function(proxy_info) {
        if (proxy_info.active && proxy_info.protocol === 'http' && proxy_info.ssl) {
            return test_proxy(log, proxy_info.url).then(function(passed_test) {
                if (passed_test) {
                    return proxy_info.url;
                } else {
                    return get_first_working_proxy(log, proxy_info_urls, index + 1);
                }
            });
        } else {
            return get_first_working_proxy(log, proxy_info_urls, index + 1);
        }
    });
}

function get_proxy_info(log, url) {
    return _q.nfcall(_request, { url: url }).spread(function(res, html) {
        if (res.statusCode != 200) {
            log.error(
                {
                    url: url,
                    status_code: res.statusCode,
                    status_message: res.statusMessage,
                    headers: res.headers
                },
                'HTTP request for proxy list produced an unexpected status code'
            );
            return _q.reject(new Error('HTTP request for proxy list produced status code ' + res.statusCode));
        }

        var $ = _cheerio.load(html);
        var proxy_is_active = $('.proxy-status').text().trim() === 'Active';

        var proxy_ip, proxy_port, proxy_protocol, proxy_ssl_enabled;
        $('.line').each(function() {
            var prop_name = $(this).find('.title').text().trim();
            var prop_val = $(this).find('.val').text().trim();

            if (prop_name === 'IPv4') {
                proxy_ip = prop_val;
            } else if (prop_name === 'Port') {
                proxy_port = prop_val;
            } else if (prop_name === 'Protocol') {
                proxy_protocol = prop_val;
            } else if (prop_name === 'SSL') {
                proxy_ssl_enabled = $(this).find('[title="Yes"]').length > 0;
            }
        });

        return {
            url: 'http://' + proxy_ip + ':' + proxy_port,
            active: proxy_is_active,
            protocol: proxy_protocol,
            ssl: proxy_ssl_enabled
        };
    }, function(err) {
        log.error({ url: url, err: err }, 'HTTP request for proxy list failed');
        throw err;  // Rethrow to handler further down chain
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

    return _q.nfcall(_request, request_options).spread(function(res, html) {
        return res.statusCode === 200;
    }, function(err) {
        return false;
    });
}

module.exports = {
	find_proxy: find_proxy
};