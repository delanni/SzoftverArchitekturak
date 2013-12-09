HBMAIN.controller("RevertController", function ($scope, Communicator, GlobalService) {
    $scope.$on("fileSelectionChanged", function () { $scope.populateVersions(GlobalService.selectedFile); });
    $scope.populateVersions = function (file) {
        if (file && file.versions) {
            $scope.versions = file.versions;
            $scope.selectedVersion = file.versions[0];
        }
    }
    $scope.versions = [];
});
