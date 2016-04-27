(function(exports) {
	// Evaluates the given query against the given text.
	// The query is evaluated left-to-right.
	// Operators, labeled by priority, include:
	// 1. (): Grouping
	// 2. OR: Logical or ('|' can be used as a pseudonym)
	// 2. AND: Logical and (this is the default operator)
	// TODO: Should we prioritize OR above other operators?
	exports.evaluateBooleanQuery = function(query, text) {
		var stack = exports.buildBooleanStack(query);
		for (var i = 0; i < stack.length; i++) {
			stack[i] = stack[i].toLowerCase();
		}

		// TODO: Make this properly tokenize e.g. 'node.js'
		var lcText = text.toLowerCase();
		var tokenizedText = lcText.match(/[\w'#\+]+/g);

		return exports.evaluateBooleanStack(stack, lcText, tokenizedText, {});
	};

	// Evaluates the given stack against the given terms.
	// The stack is evaluated left-to-right.
	// Operators, labeled by priority, include:
	// 1. (): Grouping
	// 2. OR: Logical or ('|' can be used as a pseudonym)
	// 2. AND: Logical and (this is the default operator)
	// TODO: Should we prioritize OR above other operators?
	exports.evaluateBooleanStack = function(stack, fullText, terms, hitMap) {
		// TODO: Handle empty terms

		var result = true;
		var applyOr = false;
		var fullTextMatch = false;

		for (var i = 0; i < stack.length; i++) {
			if (stack[i] === '"') {
				fullTextMatch = true;
			} else if (stack[i] === '|') {
				applyOr = true;
			} else if (stack[i] === '(') {
				var depth = 1;
				for (var j = i+1; j < stack.length && depth > 0; j++) {
					if (stack[j] === '(') {
						depth++;
					} else if (stack[j] === ')') {
						depth--;
					}
				}
				// No need to evaluate if we are currently false
				if (i+1 < j-1 && result === true) {
					if (applyOr) {
						result = result || 
							exports.evaluateBooleanStack(stack.slice(i+1, j-1), fullText, terms, hitMap);
						applyOr = false;
					} else {
						result = result && 
							exports.evaluateBooleanStack(stack.slice(i+1, j-1), fullText, terms, hitMap);
					}
				}
				// Remove the subquery from the eval stack
				stack.splice(i, j-i);
				i--;
			} else if (stack[i] === ')') {
				// This will only happen if there's a '()'
				break;
			} else {
				// Use hash since we're exposed to user input
				var hashVal = 'hash-' + (fullTextMatch ? 't-' : 'f-') + stack[i];
				if (!hitMap.hasOwnProperty[hashVal]) {
					if (fullTextMatch) {
						hitMap[hashVal] = (fullText.indexOf(stack[i]) >= 0);
						fullTextMatch = false;
					} else {
						hitMap[hashVal] = (terms.indexOf(stack[i]) >= 0);
					}
				}
				if (applyOr) {
					result = result || hitMap[hashVal];
					applyOr = false;
				} else {
					result = result && hitMap[hashVal];
				}
			}
		}

		return result;
	}; // End function 'evaluateBooleanStack'

	exports.buildBooleanStack = function(query) {
		var wordsToIgnore = ['a', 'in', 'the', 'to', 'of'];

		var stack = [];
		var token = '';
		var openQuote = false;
		var groupDepth = 0;

		for (var i = 0; i < query.length; i++) {
			switch (query[i]) {
				case '"':
					if (!openQuote) {
						stack.push('"');
					} else if (token.length > 0) {
						stack.push(token);
						token = '';
					}
					openQuote = !openQuote;
					break;
				case '(':
				case ')':
				case '|':
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					if (openQuote) {
						token += query[i];
					} else {
						if (token.length > 0 && token !== 'AND' && wordsToIgnore.indexOf(token) < 0) {
							if (token === 'OR') {
								stack.push('|');
							} else {
								stack.push(token);
							}
						}
						if (query[i] === '(') {
							groupDepth++;
							stack.push(query[i]);
						}
						else if (query[i] === ')') {
							if (groupDepth > 0) {
								groupDepth--;
								stack.push(query[i]);
							}
						}	else if (query[i] === '|') {
							stack.push(query[i]);
						}

						token = '';
					}
					break;
				default:
					token += query[i];
					break;
			}
		}

		if (token.length > 0) {
			stack.push(token);
		}

		for (var i = 0; i < groupDepth; i++) {
			stack.push(')');
		}

		// Remove redundant and useless operators
		for (var i = 0; i < stack.length - 1; i++) {
			if (stack[i] === '(' && stack[i+1] === '|') {
				stack.splice(i + 1, 1);
				i--;
			}
			if (stack[i] === '|' && (stack[i+1] === '|' || stack[i+1] === ')')) {
				stack.splice(i, 1);
				i--;
			}
		}
		while (stack.length > 0 && stack[0] === '(' && stack[stack.length - 1] === ')') {
			stack = stack.slice(1, stack.length - 1);
		}

		return stack;
	}; // End function 'buildBooleanStack'
})(
	(typeof(process) === 'undefined' || !process.versions) ?
		(window.common = window.common || {}) :
		exports
);

