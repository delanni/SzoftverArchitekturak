NGMAIN.controller("SearchController", function ($scope, $rootScope, Communicator, GlobalService) {
    $scope.searchInKey = "";
    $scope.searchInValue = "";
    $scope.searchInPath = "";
    $scope.performSearch = function () {
        Communicator.performSearch($scope.searchInPath, $scope.searchInKey, $scope.searchInValue).then(
            function (data) {
                $rootScope.$broadcast("searchDataArrived", data);
            }, function (error) {
                alert("Search error:\n" + JSON.stringify(error));
            });
    }
});
