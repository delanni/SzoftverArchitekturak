/// <reference path="_references.js"/>

var HBMAIN = angular.module("heribolz", function () {
    // Configure
});

/**
*  Sets the cursor to loading state or normal
*/
HBMAIN.setLoading = function (onOff) {
    if (onOff) {
        $(document.body).css("cursor", "progress");
    } else {
        $(document.body).css("cursor", "auto");
    }
};

HBMAIN.controller("RevertController", function ($scope, Communicator, GlobalService) {
    $scope.$on("fileSelectionChanged", function () { $scope.populateVersions(GlobalService.selectedFile); });
    $scope.populateVersions = function (file) {
        if (file && file.versions)
        $scope.versions = file.versions;
    }
    $scope.versions = [];
});



HBMAIN.directive("file", function () {
    return {
        scope: {
            filename: "@",
            filepath: "@",
            lastmodified: "@",
            locked: "@"
        },
        restrict: "E",
        templateUrl : "fileTemplate",
    };
});

HBMAIN.directive("folder", function () {
    return {
        scope: {
            foldername: "@",
            folderpath: "@",
            createdon: "@",
        },
        restrict: "E",
        templateUrl: "folderTemplate",
    };
});

HBMAIN.directive("project", function () {
    return {
        scope: {
            projectname: "@",
            ownername: "@"
        },
        restrict: "E",
        templateUrl: "projectTemplate",
    };
});

HBMAIN.directive("propertyfield", function () {
    return {
        scope: {
            property: "="
        },
        restrict: "E",
        templateUrl: "propertyTemplate",
    };
});