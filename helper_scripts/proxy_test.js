var proxy_lookup = require('../app_modules/proxy_lookup');

var proxy_urls = process.argv.slice(2);
proxy_urls.forEach(function(proxy_url) {
	proxy_lookup.test_proxy(null, proxy_url).then(function(result) {
		console.log('[' + proxy_url + ']: ' + result);
	});
});