/// <reference path="_references.js"/>

var HBMAIN = angular.module("heribolz", function () {
    // Configure
});

HBMAIN.factory("TestUser", function () {
    var user = new User({name:"Alex"});

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
    return files;
});

HBMAIN.controller("BrowserController", ["$scope","TestData",function ($scope,TestData) {
    /***
    currentPath: Current path
    files: Files list
    folders: Folders list
    projectName: Project name
    ***/
    $scope.folders = [];
    $scope.files = TestData;
    $scope.currentPath = (function (files) {
        var fallback = "unknown";
        for (var i = 0; i < files.length; i++) {
            for (var j = i+1 ; j < files.length; j++) {
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
}]);

HBMAIN.directive("file", function () {
    return {
        scope: {
            fileName: "@filename",
            filePath: "@filepath",
            lastModified: "@lastmodified"
        },
        restrict: "E",
        template: "<div title='{{filePath + fileName}}' >{{fileName}}</div>"
    };
});