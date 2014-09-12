var sjpAppModule = angular.module("sjpApp", []);

sjpAppModule.factory("sjpSharedService", function($rootScope) {
	var sharedService = {};
	sharedService.selectedCompany = "";
	sharedService.selectedTitle = "";
	sharedService.selectedLocation = "";
	sharedService.selectedSourceUri = "";
	sharedService.selectedFullDescription = "";
	sharedService.selectJd = function(jd, transformedFullDescription) {
		this.selectedCompany = jd.Company;
		this.selectedTitle = jd.Title;
		this.selectedLocation = jd.Location;
		this.selectedSourceUri = jd.SourceUri;
		this.selectedFullDescription = transformedFullDescription;
		$rootScope.$broadcast("jdSelected");
	};

	return sharedService;
});

sjpAppModule.controller(
	"ContentSelectionController",
	function($scope, sjpSharedService) {
		$scope.searchString = {
			text: "",
			searchTitles: true,
			searchDescriptions: true
		};

		function parseJds(jds) {
			for (var i = 0; i < jds.length; i++) {
				parseJd(jds[i]);
			}
		}

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
							if (jd.Title.toUpperCase() < company.jobs[j].Title.toUpperCase()) {
								company.jobs.splice(j, 0, jd);
								return;
							}
						}
						company.jobs.push(jd);
						return;
					} else if (jd.Company.toUpperCase() < company.name.toUpperCase()) {
						$scope.companies.splice(i, 0, {
							"name": jd.Company,
							"visible": true,
							"expanded": false,
							"jobs": [jd]
						});
						return;
					}
				}
				$scope.companies.push({
					"name": jd.Company,
					"visible": true,
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

		$scope.searchJobs = function() {
			for (var i = 0; i < $scope.companies.length; i++) {
				$scope.companies[i].visible = false;
				for (var j = 0; j < $scope.companies[i].jobs.length; j++) {
					var visible = false;
					if ($scope.searchString.text.length < 1) {
						// If there is no search text, show everything
						visible = true;
					} else {
						if ($scope.searchString.searchTitles) {
							visible = window.common.evaluateBooleanQuery(
								$scope.searchString.text,
								$scope.companies[i].jobs[j].Title
								);
						}
						if (!visible && $scope.searchString.searchDescriptions) {
							visible = window.common.evaluateBooleanQuery(
								$scope.searchString.text,
								$scope.companies[i].jobs[j].FullTextDescription
								);
						}
					}
					$scope.companies[i].jobs[j].visible = visible;
					if (visible) {
						$scope.companies[i].visible = true;
					}
				}
			}
		};

		$scope.selectJd = function(companyIndex, jobIndex) {
			var company = $scope.companies[companyIndex];
			var jd = company.jobs[jobIndex];

			company.selected = true;
			jd.selected = true;

			// TODO: Update highlighting
			/*var transformedDescription = window.common.booleanSearch.match(
				$scope.searchString.text,
				jd.FullHtmlDescription,
				"<span class='highlighted'>",
				"</span>"
				) || jd.FullHtmlDescription;
			sjpSharedService.selectJd(jd, transformedDescription);*/
			sjpSharedService.selectJd(jd, jd.FullHtmlDescription);
		};
	}
);

sjpAppModule.controller(
	"ContentDisplayController",
	function($scope, sjpSharedService) {
		$scope.company = "Madrona Venture Group";
		$scope.title = "Startup Job Search";
		$scope.location = "Madrona Venture Group is a Seattle-based VC firm"
		$scope.sourceUri = "//www.madrona.com";
		$scope.fullHtmlDescription = "Use the search box and the navigation tree to explore startup jobs";
		$scope.$on("jdSelected", function() {
			$scope.company = sjpSharedService.selectedCompany;
			$scope.title = sjpSharedService.selectedTitle;
			$scope.location = sjpSharedService.selectedLocation;
			$scope.sourceUri = sjpSharedService.selectecSourceUri;
			$scope.fullHtmlDescription = sjpSharedService.selectedFullDescription;
		});
	}
);

