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
			multiline: true
		};

		function parseJd(jd) {
			jd.visible = true;
			$scope.$apply(function() {
				for (var i = 0; i < $scope.companies.length; i++) {
					if ($scope.companies[i].name == jd.Company) {
						$scope.companies[i].jobs.push(jd);
						return;
					}
				}
				var newCompany = {};
				newCompany.name = jd.Company;
				newCompany.expanded = false;
				newCompany.jobs = [];
				newCompany.jobs.push(jd);
				$scope.companies.push(newCompany);
			});
		};
		function iterateDir(url, callback) {
			$.get(url, function(data) {
				var html = $("<div />").html(data).contents();
				html.find("a").each(function(index) {
					var childUrl = "";
					var path = $(this).attr("href").trim();
					if (path.indexOf("http") != 0 && path.charAt(0) != '/') {
						childUrl = url;
						if (childUrl.charAt(childUrl.length - 1)) {
							childUrl += "/";
						}
					}
					childUrl += path;
					callback(childUrl);
				});
			}).error(function(xhr, status, error) {
				alert("Status: " + status + "\nError: " + error);
			});
		};
		$scope.companies = [];
		iterateDir("http://127.0.0.1:8000/data", function(companyUrl) {
			iterateDir(companyUrl, function(jdUrl) {
				$.getJSON(jdUrl, parseJd).error(function(xhr, status, error) {
					alert("Status: " + status + "\nError: " + error);
				});
			})
		});

		$scope.toggleJobListView = function(company) {
			company.expanded = !company.expanded;
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
					var visible = true;
					var exp = createRegExpFromSearchString($scope.searchString);
					if (exp != null) {
						if (exp.exec($scope.companies[i].jobs[j].FullDescription) == null) {
							visible = false;
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
		$scope.company = "Company";
		$scope.title = "Title";
		$scope.sourceUri = "SourceUri";
		$scope.fullDescription = "Description";
		$scope.$on("jdSelected", function() {
			$scope.company = sjpSharedService.selectedJd.Company;
			$scope.title = sjpSharedService.selectedJd.Title;
			$scope.sourceUri = sjpSharedService.selectedJd.SourceUri;

			$scope.fullDescription = 
				sjpSharedService.selectedJd.FullDescription.replace(
					sjpSharedService.searchRegExp,
					function(val) {
						return "<span class='highlighted'>" + val + "</span>";
					});
		});
	}
);

