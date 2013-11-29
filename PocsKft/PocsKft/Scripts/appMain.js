/// <reference path="_references.js"/>

var HBMAIN = angular.module("heribolz", function () {
    // Configure
});

HBMAIN.factory("GlobalService", function () {
    return {};
});

HBMAIN.factory("TestUser", function () {
    var user = new User({ name: "Alex" });
    return user;
});

HBMAIN.factory("TestData", function () {
    /// FILES ///
    var alma = new File({
        fileName: "alma.txt",
        projectName: "almaproject",
        filePath: "/almaproject/korte/alma",
        properties: Property.fromObject(window)
    });
    var banan = new File({
        fileName: "banan.dat",
        projectName: "almaproject",
        filePath: "/almaproject/korte",
        properties: Property.fromObject({ "anyja neve": "ban'nos joe", "apja neve": "bananos janka" })
    });
    var csutka = new File({
        fileName: "almacsutka",
        projectName: "almaproject",
        filePath: "/almaproject/korte/barack/",
        properties: Property.fromObject(new Date())
    });
    /// selif ///

    /// FOLDERS ///
    var almaFolder = new File({
        fileName: "alma",
        filePath: "/almaproject/korte/",
        projectName: "almaproject",
        isRealFile: false
    });
    var barackFolder = new File({
        fileName: "barack",
        filePath: "/almaproject/korte",
        projectName: "almaproject",
        isRealFile: false
    });
    var korte = new File({
        fileName: "korte",
        filePath: "/almaproject",
        projectName: "almaproject",
        isRealFile: false
    });
    var almaproject = new Project({
        projectName: "almaproject",
        ownerName: "alex",
    });
    /// sredlof ///

    return {
        listFolder: function (folderName) {
            if (folderName === '/') return [almaproject];
            if (folderName === '/almaproject') return [
                korte
            ]; else if (folderName === '/almaproject/korte') return [
                barackFolder,
                almaFolder,
                banan
            ]; else if (folderName === '/almaproject/korte/alma') return [
                alma
            ]; else if (folderName === '/almaproject/korte/barack') return [
                csutka
            ];
        }
    };
});

HBMAIN.factory("Communicator", function () {
    var c = {};
    return {
        listFolder: function (folderName) {

        }
    };
});

HBMAIN.controller("BrowserController", ["$scope", "TestData", "GlobalService", function ($scope, TestData, GlobalService) {
    $scope.reload = function () {
        $scope.currentPath = window.location.pathname || "/";
        $scope.files = TestData.listFolder($scope.currentPath);
        $scope.projectName = (function (files) {
            var fallback = "unknown";
            for (var i = 0; i < files.length; i++) {
                for (var j = i + 1 ; j < files.length; j++) {
                    if (files[i].projectName === files[j].projectName) return files[i].projectName;
                }
            }
            return fallback;
        })($scope.files);
        $scope.clearSelections();
    };

    $scope.select = function (selected, event) {
        if (selected.selected && !selected.isRealFile) {
            delete GlobalService.selectedFile;
            $scope.currentPath = selected.filePath + (selected.fileName || selected.projectName);
            window.history.pushState($scope.currentPath, null, $scope.currentPath);
            selected.selected = false;
            $scope.reload();
        } else {
            $scope.clearSelections();
            selected.selected = true;
            GlobalService.selectedFile = selected;
            setTimeout(window.resize, 500);
        }
        return true;
    };

    $scope.clearSelections = function (event) {
        if (event) { event.bubbles = true; event.handled = false; }
        for (var i in $scope.files) { $scope.files[i].selected = false; }
        delete GlobalService.selectedFile;
    }

    $(window).on('popstate', function (e) {
        $scope.currentPath = e.originalEvent.state;
        $scope.$apply(function () { $scope.reload() });
        window.resize();
    });

    $scope.reload();
}]);

HBMAIN.controller("PropertiesController", ["$scope", "TestData", "GlobalService", function ($scope, TestData, GlobalService) {
    $scope.global = GlobalService;

    getTarget = function () {
        return GlobalService.selectedFile;
    }
    $scope.evaluateLockStatus = function () {
        var target = getTarget();
        if (!target) {
            $scope.displayHelpMessage(true);
            return;
        } else if (!target.hasOwnProperty("lockStatus")) {
            $scope.displayHelpMessage(false);
        }

        switch (target.lockStatus) {
            case "UNAUTHORIZED":
                $scope.lockMessage = "You don't have the rights to assume file lock"
                $scope.lockEnabled = false;
                break;
            case "LOCKED":
                $scope.lockMessage = "The file is currently locked"
                $scope.lockEnabled = false;
                break;
            case "UNLOCKED":
                $scope.lockMessage = "Assume the lock of the file"
                $scope.lockEnabled = true;
                break;
            default:
                break;
        }
    };

    $scope.assumeControl = function () {

    };
}]);

HBMAIN.controller("ActionBarController", ["$scope", "TestData", "GlobalService", function ($scope, TestData, GlobalService) {
    $scope.global = GlobalService;
    $scope.execute = function (action) {
        action.execute();
    }
    ACTIONS = {
        DELETE: {
            name: "Delete file",
            type: "bin",
            execute: function () { console.log("deleting file"); }
        },
        EDIT: {
            name: "Edit / Rename file",
            type: "pencil",
            execute: function () { console.log("renaming file"); }
        },
        UPLOAD: {
            name: "Upload new version of the file",
            type: "upload",
            execute: function () { console.log("uploading file"); }
        },
        DOWNLOAD: {
            name: "Download file",
            type: "download",
            execute: function () { console.log("downloading file"); }
        },
        LOCK: {
            name: "Try to acquire file lock",
            type: "lock",
            execute: function () { console.log("file locking");}
        }
    };
    
    $scope.actions = [
        ACTIONS.DELETE, ACTIONS.EDIT, ACTIONS.UPLOAD, ACTIONS.DOWNLOAD, ACTIONS.LOCK
    ];
}]);

HBMAIN.directive("file", function () {
    return {
        scope: {
            fileName: "@filename",
            filePath: "@filepath",
            lastModified: "@lastmodified",
        },
        restrict: "E",
        template: "<div class='file' title='{{filePath + fileName}}' >{{fileName}}</div>",
    };
});

HBMAIN.directive("folder", function () {
    return {
        scope: {
            folderName: "@foldername",
            folderPath: "@folderpath",
            createdOn: "@createdon",
        },
        restrict: "E",
        template: "<div class='folder' alt='{{folderPath + folderName}}'>[{{folderName}}]</div>",
    };
});

HBMAIN.directive("project", function () {
    return {
        scope: {
            projectName: "@projectname",
            ownerName: "@ownername"
        },
        restrict: "E",
        template: "<div class='project'>*{{projectName}}*</div>",
    };
});