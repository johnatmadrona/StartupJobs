var _q = require('q');
var _cheerio = require('cheerio');
var _nodemailer = require('nodemailer');

function create(host, port, sender_email, sender_pwd, /* optional */ default_recipients) {
	var transporter = _nodemailer.createTransport({
		host: host,
		port: port,
		auth: {
			user: sender_email,
			pass: sender_pwd
		}
	});

	return {
		send: function(subject, html_message, /* optional */ recipients) {
			var r = recipients;
			if (typeof(r) === 'undefined' || r === null) {
				r = default_recipients;
			}
			if (typeof(r) === 'undefined' || r === null) {
				return _q().reject(new Error('Must specify recipients or set default_recipients on creation'));
			}

			var doc = _cheerio.load('<span></span>')('span').append(html_message);

			var mail_options = {
			    from: sender_email,
			    to: r,
			    subject: subject,
			    text: doc.text(),
			    html: doc.html()
			};

			return _q.ninvoke(transporter, 'sendMail', mail_options);
		}
	};
}

module.exports = {
	create: create
};