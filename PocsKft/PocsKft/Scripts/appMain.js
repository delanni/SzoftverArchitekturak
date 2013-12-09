/// <reference path="_references.js"/>

var NGMAIN = angular.module("softbolz", function () {
    // Configure if needed
});

/**
*  Sets the cursor to loading state or normal
*/
NGMAIN.setLoading = function (onOff) {
    if (onOff) {
        $(document.body).css("cursor", "progress");
    } else {
        $(document.body).css("cursor", "auto");
    }
};

NGMAIN.directive("file", function () {
    return {
        scope: {
            filename: "@",
            filepath: "@",
            lastmodified: "@",
            locked: "@"
        },
        restrict: "E",
        templateUrl: "fileTemplate",
    };
});

NGMAIN.directive("folder", function () {
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

NGMAIN.directive("project", function () {
    return {
        scope: {
            projectname: "@",
            ownername: "@"
        },
        restrict: "E",
        templateUrl: "projectTemplate",
    };
});

NGMAIN.directive("propertyfield", function () {
    return {
        scope: {
            property: "="
        },
        restrict: "E",
        templateUrl: "propertyTemplate",
    };
});