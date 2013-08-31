var sjpAppModule = angular.module("sjpApp", []);

sjpAppModule.factory("sjpSharedService", function($rootScope) {
	var sharedService = {};
	sharedService.selectedJd = {};
	sharedService.searchRegExp = {};
	sharedService.selectJd = function(jd, searchRegExp) {
		this.selectedJd = jd;
		this.searchRegExp = searchRegExp;
		$rootScope.$broadcast("jdSelected");
	};

	return sharedService;
});

sjpAppModule.controller(
	"ContentSelectionController",
	function($scope, sjpSharedService) {
		$scope.searchString = {
			text: "",
			caseInsensitive: true,
			global: true,
			multiline: true,
			searchTitles: true,
			searchDescriptions: true
		};

		function parseJd(jd) {
			jd.visible = true;
			$scope.$apply(function() {
				for (var i = 0; i < $scope.companies.length; i++) {
					var company = $scope.companies[i];
					if (company.name == jd.Company) {
						// Sorted insertion because Angular doesn't seem to 
						// properly support sorted nested lists
						// TODO: Investigate whether this can be addressed 
						// in Angular instead of here
						for (var j = 0; j < company.jobs.length; j++) {
							if (jd.Title < company.jobs[j].Title) {
								company.jobs.splice(j, 0, jd);
								return;
							}
						}
						company.jobs.push(jd);
						return;
					} else if (jd.Company < company.name) {
						$scope.companies.splice(i, 0, {
							"name": jd.Company,
							"expanded": false,
							"jobs": [jd]
						});
						return;
					}
				}
				$scope.companies.push({
					"name": jd.Company,
					"expanded": false,
					"jobs": [jd]
				});
			});
		}

		function iterateAwsDir(path, callback) {
			if (path.charAt(path.length - 1) != '/') {
				path += "/";
			}
			var url = "/?prefix=" + encodeURIComponent(path) + "&delimiter=%2F";
			$.get(url, function(data) {
				var xml = $(data);
				var commonPrefixes = xml.find("CommonPrefixes");
				if (commonPrefixes.length > 0) {
					commonPrefixes.each(function() {
						$(this).find("Prefix").each(function() {
							callback($(this).text());
						});
					});
				}
				var contents = xml.find("Contents");
				if (contents.length > 0) {
					contents.each(function() {
						callback($(this).find("Key").text());
					});
				}
			}).error(function(xhr, status, error) {
				alert("Status: " + status + "\nError: " + error);
			});
		}

		function iterateLocalDir(path, callback) {
			if (path.charAt(path.length - 1) != '/') {
				path += "/";
			}
			var url = "./" + path;
			$.get(url, function(data) {
				var html = $("<div />").html(data).contents();
				html.find("a").each(function(index) {
					var childUrl = "";
					var childPath = $(this).attr("href").trim();
					if (childPath.indexOf("http") != 0 && childPath.charAt(0) != '/') {
						childUrl = url;
						if (childUrl.charAt(childUrl.length - 1) != '/') {
							childUrl += "/";
						}
					}
					childUrl += childPath;
					callback(childUrl);
				});
			}).error(function(xhr, status, error) {
				alert("Status: " + status + "\nError: " + error);
			});
		}

		$scope.companies = [];
		var iterateDir = iterateAwsDir;
		if ($(location).attr("href").indexOf("127.0.0.1") >= 0) {
			iterateDir = iterateLocalDir
		}
		iterateDir("data/", function(companyUrl) {
			iterateDir(companyUrl, function(jdUrl) {
				$.getJSON(jdUrl, parseJd).error(function(xhr, status, error) {
					alert("Status: " + status + "\nError: " + error);
				});
			})
		});

		$scope.toggleJobListView = function(company) {
			company.expanded = !company.expanded;
		};

		$scope.toggleAllCausesExpansion = true;
		$scope.toggleAllCompanyJobListViews = function() {
			for (var i = 0; i < $scope.companies.length; i++) {
				$scope.companies[i].expanded = $scope.toggleAllCausesExpansion;
			}
			$scope.toggleAllCausesExpansion = !$scope.toggleAllCausesExpansion;
		};

		function createRegExpFromSearchString(searchString) {
			if ($scope.searchString.text.length < 1) {
				return null;
			}

			var options = "";
			if (searchString.caseInsensitive) {
				options += "i";
			}
			if (searchString.global) {
				options += "g";
			}
			if (searchString.multiline) {
				options += "m";
			}

			return new RegExp(searchString.text, options);
		}

		$scope.searchJobs = function() {
			for (var i = 0; i < $scope.companies.length; i++) {
				for (var j = 0; j < $scope.companies[i].jobs.length; j++) {
					var visible = false;
					var exp = createRegExpFromSearchString($scope.searchString);
					if (exp == null) {
						visible = true;
					} else {
						if ($scope.searchString.searchTitles && 
							exp.exec($scope.companies[i].jobs[j].Title) != null) {
							visible = true;
						} else  if ($scope.searchString.searchDescriptions && 
							exp.exec($scope.companies[i].jobs[j].FullTextDescription) != null) {
							visible = true;
						}
					}
					$scope.companies[i].jobs[j].visible = visible;
				}
			}
		};

		$scope.selectJd = function(company, job) {
			sjpSharedService.selectJd(
					$scope.companies[company].jobs[job],
					createRegExpFromSearchString($scope.searchString));
		};
	}
);

sjpAppModule.controller(
	"ContentDisplayController",
	function($scope, sjpSharedService) {
		$scope.company = "Madrona Venture Group";
		$scope.title = "Startup Job Search";
		$scope.sourceUri = "#";
		$scope.fullHtmlDescription = "Use the search box and the navigation tree to explore startup jobs";
		$scope.$on("jdSelected", function() {
			$scope.company = sjpSharedService.selectedJd.Company;
			$scope.title = sjpSharedService.selectedJd.Title;
			$scope.sourceUri = sjpSharedService.selectedJd.SourceUri;

			$scope.fullHtmlDescription = 
				sjpSharedService.selectedJd.FullHtmlDescription.replace(
					sjpSharedService.searchRegExp,
					function(val) {
						return "<span class='highlighted'>" + val + "</span>";
					});
		});
	}
);

