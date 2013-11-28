/// <reference path="_references.js"/>

var HBMAIN = angular.module("heribolz", function () {
    // Configure
});

HBMAIN.factory("TestUser", function () {
    var user = new User({ name: "Alex" });

    return user;
});

HBMAIN.factory("TestData", function () {
    var files = [
            new File({
                fileName: "alma.txt",
                projectName: "almaproject",
                filePath: "/almaproject/korte/alma"
            }),
            new File({
                fileName: "banan.dat",
                projectName: "almaproject",
                filePath: "/almaproject/korte/alma"
            }),
            new File({
                fileName: "almacsutka",
                projectName: "almaproject",
                filePath: "/almaproject/korte/barack/"
            }),
    ];
    var folders = [
        new File({
            fileName: "Potato",
            filePath: "/almaproject/korte/alma",
            projectName: "almaproject"
        }),
    ];

    var properties = {
        targetName: "selected file name.txt",
        properties: [
            new Property({
                propName: "this property name",
                propValue: "this property value"
            }),
            Property.fromObject({
                abra: "kadabra",
                bingidi: "boom",
            })[0]
        ]
    };

    return {
        files: files,
        folders: folders,
        properties: properties
    };
});

HBMAIN.controller("BrowserController", ["$scope", "TestData", function ($scope, TestData) {
    /***
    currentPath: Current path
    files: Files list
    folders: Folders list
    projectName: Project name
    ***/
    $scope.folders = [];
    $scope.files = TestData.files;
    $scope.folders = TestData.folders;
    $scope.getAll = function () { return $scope.files.concat($scope.folders);}
    $scope.currentPath = (function (files) {
        var fallback = "unknown";
        for (var i = 0; i < files.length; i++) {
            for (var j = i + 1 ; j < files.length; j++) {
                if (files[i].fileName === files[j].fileName) return files[i].fileName;
            }
        }
        return fallback;
    })($scope.files);
    $scope.projectName = (function (files) {
        var fallback = "unknown";
        for (var i = 0; i < files.length; i++) {
            for (var j = i + 1 ; j < files.length; j++) {
                if (files[i].projectName === files[j].projectName) return files[i].projectName;
            }
        }
        return fallback;
    })($scope.files);

    $scope.select = function (selected) {
        if (selected.selected) console.log("navigation not supported, sit tight");
        for (var i in $scope.getAll()) { $scope.getAll()[i].selected = false; }
        selected.selected = true;
    };
}]);

HBMAIN.controller("PropertiesController", ["$scope", "TestData", function ($scope, TestData) {
    $scope.targetName = TestData.properties.targetName;
    $scope.properties = TestData.properties.properties;
}]);

HBMAIN.controller("ActionBarController", ["$scope", "TestData", function ($scope, TestData) {
}]);

HBMAIN.directive("file", function () {
    return {
        scope: {
            fileName: "@filename",
            filePath: "@filepath",
            lastModified: "@lastmodified",
            selected: "@selected"
        },
        restrict: "E",
        template: "<div title='{{filePath + fileName}}' >{{fileName}}</div>",
        link: function (scope, element) {
            scope.selected ? $(element).addClass("selected") : $(element).removeClass("selected");
        }
    };
});

HBMAIN.directive("folder", function () {
    return {
        scope: {
            folderName: "@foldername",
            folderPath: "@folderpath",
            createdOn: "@createdon",
            selected: "@selected"
        },
        restrict: "E",
        template: "<div>[{{folderPath}}]</div>",
    };
});