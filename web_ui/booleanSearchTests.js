var bs = require('./booleanSearch.js');

var testQuery = 'c# (dev OR engineer)';
var expected = ['c#', '(', 'dev', '|', 'engineer', ')'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# (dev OR engineer';
expected = ['c#', '(', 'dev', '|', 'engineer', ')'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# (dev OR engineer) in test';
expected = ['c#', '(', 'dev', '|', 'engineer', ')', 'test'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# "software engineer';
expected = ['c#', '"', 'software engineer'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# "software engineer" in test';
expected = ['c#', '"', 'software engineer', 'test'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# software (engineer|developer) in test';
expected = ['c#', 'software', '(', 'engineer', '|', 'developer', ')', 'test'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# "software (engineer|developer)" in test';
expected = ['c#', '"', 'software (engineer|developer)', 'test'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# software (engineer|developer|dev) "in test"';
expected = ['c#', 'software', '(', 'engineer', '|', 'developer', '|', 'dev', ')', '"', 'in test'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = '(c# (software (eng|dev';
expected = ['c#', '(', 'software', '(', 'eng', '|', 'dev', ')', ')'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# software (eng||dev)';
expected = ['c#', 'software', '(', 'eng', '|', 'dev', ')'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = 'c# software (|OR|eng|dev|OR)';
expected = ['c#', 'software', '(', 'eng', '|', 'dev', ')'];
runBuildBooleanStackTest(testQuery, expected);

testQuery = '((c# software))';
expected = ['c#', 'software'];
runBuildBooleanStackTest(testQuery, expected);

function runBuildBooleanStackTest(query, expectedResult) {
	var result = bs.buildBooleanStack(query);
	var success = arraysAreEqual(result, expectedResult);
	console.log((success ? 'PASS' : 'FAIL') + ' on buildBooleanStack for query "' + query + '"');
	if (!success) {
		console.log('  Expected: ' + expectedResult);
		console.log('    Actual: ' + result);
	}
}

var testText = 'We\'re looking to hire a top-notch C# developer to build a scalable web service';

testQuery = '"c# developer"';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = 'web service "c# developer"';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = '(engineer|developer)';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = 'web (engineer|developer)';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = 'web engineer|developer';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = 'c# web (engineer|developer)';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = 'c# web (eng|dev)';
runEvaluateBooleanQueryTest(testQuery, testText, false);

testQuery = 'c#|(c++|c code)(web|"full stack" developer) service';
runEvaluateBooleanQueryTest(testQuery, testText, true);

testQuery = '(c#|java code)(web|"full stack" developer) service';
runEvaluateBooleanQueryTest(testQuery, testText, false);

testQuery = '(c#|java)(web|"full stack" developer) service';
runEvaluateBooleanQueryTest(testQuery, testText, true);

function runEvaluateBooleanQueryTest(query, text, expectedResult) {
	var result = bs.evaluateBooleanQuery(query, text);
	console.log(
		(result === expectedResult ? 'PASS' : 'FAIL') + 
		' on evaluateBooleanQuery for query "' + query + 
		'" and text "' + text + '"'
		);
}

function arraysAreEqual(a1, a2) {
	if (a1.length != a2.length) {
		return false;
	}

	for (var i = 0; i < a1.length; i++) {
		if (a1[i] !== a2[i]) {
			return false;
		}
	}

	return true;
}

