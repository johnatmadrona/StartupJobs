var request = require('request');
var cheerio = require('cheerio');
var fs = require('fs');
var path = require('path');

console.log('Requesting portfolio page...');
request('http://www.madrona.com/portfolio/', function(err, res, html) {
	if (err) {
		console.log(err);
	} else {
		console.log('Got page, downloading images...');
		var $ = cheerio.load(html);
		downloadImages($('img[src$="png"]');
		downloadImages($('img[src$="jpg"]'));
	}
});

function downloadImages(links) {
	links.each(function() {
		var url = $(this).attr('src');
		var filename = path.basename(url);
		console.log('Downloading ' + url + ' to ' + filename);
		request(url).pipe(fs.createWriteStream(filename)).on('close', function() {
			console.log('Finished downloading ' + url);
		});
	});
}
