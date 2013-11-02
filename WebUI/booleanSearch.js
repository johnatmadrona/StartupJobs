(function(exports) {
	exports.booleanSearch = {
		// Parse query string into tokens. Returns an array of tokens.
		// Allowable tokens are unbroken strings of letters, number, and/or 
		// characters '*', '+', '.', and '#'.
		// Quotes will treat enclosed text as a single token.
		parseQuery: function(query) {
			var match, exp = /"|[a-zA-Z0-9\*\+\.#]+/gm, openQuote = false, tokens = [];
			while (match = exp.exec(query)) {
				var newText = match[0];
				if (newText === "\"") {
					openQuote = !openQuote;
					if (openQuote) {
						tokens.push("");
					} else {
						// Trim
						tokens[tokens.length - 1] = tokens[tokens.length - 1].replace(/^\s+|\s+$/gm, "");
					}
				} else if (openQuote) {
						tokens[tokens.length - 1] += newText + " ";
				} else {
					tokens.push(newText);
				}
			}

			if (openQuote) {
				// Trim
				tokens[tokens.length - 1] = tokens[tokens.length - 1].replace(/^\s+|\s+$/gm, "");
			}

			for (var i = 0; i < tokens.length; i++) {
				tokens[i] = tokens[i].replace("+", "\\+");
				tokens[i] = tokens[i].replace(".", "\\.");
				tokens[i] = tokens[i].replace("*", "\\w+");
			}

			return tokens;
		},

		// Runs the query against the doc to test for a match.
		// To be a match, all terms in the query much exist in the doc.
		// If it is not a match, null is returned.
		// If it is a match, the matches will be prepended with 
		// matchPrepend and appended with matchAppend and the resulting 
		// document returned.
		match: function(query, doc, caseInsensitive, matchPrepend, matchAppend) {
			if (typeof(doc) === "undefined" || doc.length < 1) {
				return null;
			}

			var tokens = this.parseQuery(query);
			if (tokens.length < 1) {
				return null;
			}

			var regexOptions = "gm" + (caseInsensitive ? "i" : "");

			var replaceText = "";
			for (var i = 0; i < tokens.length; i++) {
				// TODO: Parameterize case sensitivity
				var regex = new RegExp(tokens[i], regexOptions);
				if (!regex.test(doc)) {
					return null;
				}
				replaceText += tokens[i] + "|";
			}
			replaceText = replaceText.substring(0, replaceText.length - 1);

			return doc.replace(
				// TODO: Parameterize case sensitivity
				new RegExp(replaceText, regexOptions),
				function(match) {
					var result = "";
					if (typeof(matchPrepend) !== "undefined" && matchPrepend.length > 0) {
						result += matchPrepend;
					}
					result += match;
					if (typeof(matchAppend) !== "undefined" && matchAppend.length > 0) {
						result += matchAppend;
					}
					return result;
				}
			);
		}
	} // End object 'booleanSearch'
})(
	(typeof(process) === "undefined" || !process.versions) ? 
		(window.common = window.common || {}) : 
		exports
);

