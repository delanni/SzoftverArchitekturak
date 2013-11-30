/// <reference path="_references.js"/>

var HBMAIN = angular.module("heribolz", function () {
    // Configure
});

HBMAIN.setLoading = function (onOff) {
    if (onOff) {
        $(document.body).css("cursor", "progress");
    } else {
        $(document.body).css("cursor", "auto");
    }
}

HBMAIN.factory("GlobalService", function (Communicator) {
    ACTIONS = {
        DELETE: {
            name: "Delete file",
            type: "bin",
            execute: function (file) {
                Communicator.delete(file);
            }
        },
        EDIT: {
            name: "Edit / Rename file",
            type: "pencil",
            execute: function () {
                var display = $(".edit").css("display");
                if (display === 'none') {
                    $(".edit").slideDown(100);
                } else {
                    $(".edit").slideUp(100);
                }
            }
        },
        UPLOAD: {
            name: "Upload new version of the file / new file",
            type: "upload",
            execute: function (file) {
                if (file) {
                    Communicator.updateFile(file);
                } else {
                    Communicator.uploadFile();
                }
            }
        },
        DOWNLOAD: {
            name: "Download file",
            type: "download",
            execute: function (file) {
                Communicator.download(file);
            }
        },
        DOLOCK:{
            name: "Try to acquire lock on the file",
            type: "lock",
            execute: function (file) {
                Communicator.tryLock(file);
            }
        },
        UNLOCK:{
            name: "Release the lock",
            type: "unlock",
            execute: function (file) {
                Communicator.unlock(file);
            }
        },
        NOPERMLOCK:{
            name: "You have no rights to lock the file",
            type: "warning_sign",
            execute: function () {

            }
        },
        LOCKED: {
            name: "The file is currently locked",
            type: "rotation_lock",
            execute: function(){
                
            }
        },
        SAVE: {
            name: "Save editions on the file",
            type: "save",
            execute: function (file) {
                Communicator.updateMeta(file);
            }
        }
    };
    ACTIONS.LOCK = ACTIONS.NOPERMLOCK;

    return {
        ACTIONS: ACTIONS
    };
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

HBMAIN.factory("Communicator", function ($http, $q, TestData) {
    var c = {};
    var fakeList = function (scope, folderName) {
        var deferred = $q.defer();

        setTimeout(function () {
            scope.$apply(function () {
                deferred.resolve(TestData.listFolder(folderName));
            });
        }, 500);
        HBMAIN.setLoading(true);
        return deferred.promise;
    };

    var listAsync = function (scope, folderName) {
        var deferred = $q.defer();
        var url = folderName;

        $http.get(url).success(function (data) {
            deferred.resolve(data);
        }).error(function (data, status) {
            data = data || "Folder request failed";
            deferred.reject(data);
        });

        HBMAIN.setLoading(true);
        return deferred.promise;
    };

    return {
        listFolder: fakeList,
        delete: angular.noop,
        updateFile: angular.noop,
        uploadFile: angular.noop,
        updateMeta: angular.noop,
        download: angular.noop,
        tryLock: angular.noop,
        unlock: angular.noop
    };
});

HBMAIN.controller("BrowserController", ["$scope", "Communicator","GlobalService", function ($scope, Communicator, GlobalService) {
    $scope.reload = function () {
        $scope.currentPath = window.location.pathname || "/";
        Communicator.listFolder($scope, $scope.currentPath).then(function (data) {
            $scope.files = data;
            $scope.clearSelections();
            HBMAIN.setLoading(false);
        });
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

HBMAIN.controller("PropertiesController", ["$scope", "Communicator", "GlobalService", function ($scope, Communicator, GlobalService) {
    $scope.global = GlobalService;

    $scope.$watch($scope.global.selectedFile, function (oldval, newVal) {
        if (newVal) {
            $scope.$apply(function () {
                $scope.evaluateLockStatus(newVal);
            });
        }
    });

    $scope.evaluateLockStatus = function (target) {
        if (!target) {
            GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.NOPERMLOCK;
            return;
        }

        switch (target.lockStatus) {
            case "UNDERCONTROL":
                GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.UNLOCK;
                break;
            case "UNAUTHORIZED":
                GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.NOPERMLOCK;
                break;
            case "LOCKED":
                GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.LOCKED;
                break;
            case "UNLOCKED":
                GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.DOLOCK;
                break;
            default:
                GlobalService.ACTIONS.LOCK = GlobalService.ACTIONS.NOPERMLOCK;
                break;
        }
    };

    getTarget = function () {
        return GlobalService.selectedFile;
    };
}]);

HBMAIN.controller("ActionBarController", ["$scope", "Communicator", "GlobalService", function ($scope, Communicator, GlobalService) {
    $scope.global = GlobalService;
    
    $scope.execute = function (action) {
        action.execute(GlobalService.selectedFile);
    }

    $scope.actions = [
       GlobalService.ACTIONS.DELETE,
       GlobalService.ACTIONS.EDIT,
       GlobalService.ACTIONS.UPLOAD,
       GlobalService.ACTIONS.DOWNLOAD,
       GlobalService.ACTIONS.LOCK
    ];
}]);

HBMAIN.directive("file", function () {
    return {
        scope: {
            filename: "@",
            filepath: "@",
            lastmodified: "@",
        },
        restrict: "E",
        template: "<div class='file' alt='{{filepath}}' title='{{filepath + filename}}' >{{filename}}</div>",
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
        template: "<div class='folder' alt='{{folderpath + foldername}}'>[{{foldername}}]</div>",
    };
});

HBMAIN.directive("project", function () {
    return {
        scope: {
            projectname: "@",
            ownername: "@"
        },
        restrict: "E",
        template: "<div alt='Owner: {{ownername}}' class='project'>*{{projectname}}*</div>",
    };
});

HBMAIN.directive("propertyfield", function () {
    var propertyTemplate = "<div class='property'>\
                            <div class='propname' ng-bind='property.propName' />\
                            <input class='edit propname' ng-model='property.propName'/>\
                            <div class='propvalue' ng-bind='property.propValue' />\
                            <input class='edit propvalue' ng-model='property.propValue' />\
                            </div>";
    return {
        scope: {
            property:"="
        },
        restrict: "E",
        template: propertyTemplate,
    }
});