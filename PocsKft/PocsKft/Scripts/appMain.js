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
};

HBMAIN.factory("GlobalService", function (Communicator) {
    ACTIONS = {
        DELETE: {
            name: "Delete",
            type: "bin",
            execute: function (file) {
                Communicator.delete(file);
            }
        },
        EDIT: {
            name: "Edit / Rename",
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
            name: "Upload a new file",
            type: "upload",
            execute: function (file) {
                Communicator.uploadFile();
            }
        },
        UPDATE: {
            name: "Upload new version",
            type: "git_commit",
            execute: function (file) {
                Communicator.updateFile(file);
            }
        },
        DOWNLOAD: {
            name: "Download",
            type: "download",
            execute: function (file) {
                Communicator.download(file);
            }
        },
        LOCK: {
            name: "Try to acquire lock on the file",
            type: "lock",
            execute: function (file) {
                Communicator.tryLock(file);
            }
        },
        UNLOCK: {
            name: "Release the lock",
            type: "unlock",
            execute: function (file) {
                Communicator.unlock(file);
            }
        },
        LOCKED: {
            name: "The file is currently locked",
            type: "rotation_lock",
            execute: function () {
            }
        },
        SAVE: {
            name: "Save changes",
            type: "floppy_disk",
            execute: function (file) {
                $(".edit").slideUp(100);
                Communicator.updateMeta(file);
            }
        },
        CREATEPROJECT: {
            name: "Create a new project here",
            type: "book_open",
            execute: function () {
                Communicator.createProject();
            }
        },
        CREATEFOLDER: {
            name: "Create a new folder here",
            type: "folder_new",
            execute: function () {
                Communicator.createFolder();
            }
        }
    };

    ACTIONS.getWriteActionsInLobby = function () {
        return [ACTIONS.CREATEPROJECT];
    };
    ACTIONS.getReadActionsInLobby = function () {
        return [];
    };
    ACTIONS.getWriteActionsForProject = function () {
        return [ACTIONS.EDIT, ACTIONS.SAVE, ACTIONS.DELETE];
    };
    ACTIONS.getReadActionsForProject = function () {
        return [ACTIONS.DOWNLOAD];
    };
    ACTIONS.getWriteActionsInFolder = function () {
        return [ACTIONS.CREATEFOLDER, ACTIONS.UPLOAD];
    };
    ACTIONS.getReadActionsInFolder = function () {
        return [];
    };
    ACTIONS.getWriteActionsForFolder = function () {
        return [ACTIONS.EDIT, ACTIONS.SAVE, ACTIONS.DELETE];
    };
    ACTIONS.getReadActionsForFolder = function () {
        return [ACTIONS.DOWNLOAD];
    };
    ACTIONS.getWriteActionsForFile = function (file) {
        var baseActions = [ACTIONS.EDIT, ACTIONS.SAVE, ACTIONS.DELETE];
        if (file.lockStatus === 'UNLOCKED') {
            return [ACTIONS.LOCK].concat(baseActions);
        } else if (file.lockStatus === 'LOCKED') {
            return [ACTIONS.LOCKED];
        } else {
            return [ACTIONS.UPDATE, ACTIONS.UNLOCK].concat(baseActions);
        }
    };
    ACTIONS.getReadActionsForFile = function (file) {
        return [ACTIONS.DOWNLOAD];
    };

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
        lockStatus: "LOCKED",
        filePath: "/almaproject/korte",
        properties: Property.fromObject({ "anyja neve": "ban'nos joe", "apja neve": "bananos janka" })
    });
    var csutka = new File({
        fileName: "almacsutka",
        projectName: "almaproject",
        lockStatus: "UNLOCKED",
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

    var root = { files: [almaproject] };
    almaproject.files = [korte];
    korte.files = [barackFolder, almaFolder, banan];
    barackFolder.files = [csutka];
    almaFolder.files = [alma];

    return {
        listFolder: function (folderName) {
            var current = root;
            folderName = folderName.substring(1, folderName.length);
            var path = folderName.split("/");
            for (var i = 0; i < path.length; i++) {
                var name = path[i];
                if (name === "") {
                    return current.files;
                } else {
                    current = current.files[current.files.map(function (e) { return e.fileName || e.projectName; }).indexOf(name)];
                }
            }
            return current.files;
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
            if (data && data.hasOwnProperty("length")) {
                deferred.resolve(data);
            } else if(!data) {
                deferred.resolve([]);
            } else {
                deferred.resolve([data]);
            }
        }).error(function (data, status) {
            data = data || "Folder request failed";
            deferred.reject(data);
        });

        HBMAIN.setLoading(true);
        return deferred.promise;
    };

    createFolderFake = function () {
        $("#folderDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Create": function () {
                    var folderName = $("#folderNameBox").val();
                    // validate
                    // actually create
                    $("#folderNameBox").val("");
                    $(this).dialog("close");
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    createProjectFake = function () {
        $("#projectDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Create": function () {
                    var folderName = $("#projectNameBox").val();
                    // validate 
                    // actually create
                    $("#projectNameBox").val("");
                    $(this).dialog("close");
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    return {
        listFolder: listAsync,
        delete: angular.noop,
        updateFile: angular.noop,
        uploadFile: angular.noop,
        updateMeta: angular.noop,
        download: angular.noop,
        tryLock: angular.noop,
        unlock: angular.noop,
        createProject: createProjectFake,
        createFolder: createFolderFake
    };
});

HBMAIN.controller("BrowserController", ["$scope", "$rootScope", "Communicator", "GlobalService",
    function ($scope, $rootScope, Communicator, GlobalService) {
        $scope.reload = function () {
            $scope.currentPath = window.location.pathname || "/#";
            if ($scope.currentPath === "/") {
                $scope.currentPath = "/null";
            }
            GlobalService.currentPath = $scope.currentPath;
            $rootScope.$broadcast("pathChanged", $scope.currentPath);

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
                $rootScope.$broadcast("fileSelectionChanged", selected);
                setTimeout(window.resize, 500);
            }
            return true;
        };

        $scope.clearSelections = function (event) {
            if (event) { event.bubbles = true; event.handled = false; }
            for (var i in $scope.files) { $scope.files[i].selected = false; }
            delete GlobalService.selectedFile;
            $rootScope.$broadcast("fileSelectionChanged", null);
        };

        $(window).on('popstate', function (e) {
            $scope.currentPath = e.originalEvent.state;
            $scope.$apply(function () { $scope.reload(); });
            window.resize();
        });

        $scope.reload();
    }]);

HBMAIN.controller("PropertiesController", ["$scope", "$rootScope", "Communicator", "GlobalService",
    function ($scope, $rootScope, Communicator, GlobalService) {
        $scope.global = GlobalService;

        $scope.$on("fileSelectionChanged", function (event) {
        });


    }]);

HBMAIN.controller("ActionBarController", ["$scope", "Communicator", "GlobalService", function ($scope, Communicator, GlobalService) {
    $scope.global = GlobalService;

    ACTIONS = GlobalService.ACTIONS;

    $scope.$on("fileSelectionChanged", function (event) {
        if (GlobalService.selectedFile) {
            if (GlobalService.selectedFile.isRealFile) {
                setFileActions(GlobalService.selectedFile);
            } else if (GlobalService.selectedFile.hasOwnProperty("isRealFile")) {
                setFolderActions(GlobalService.selectedFile);
            } else {
                setProjectActions(GlobalService.selectedFile);
            }
        } else {
            if (GlobalService.currentPath === '/') {
                setNoTargetActionsProject();
            } else {
                setNoTargetActions();
            }
        }
    });

    setFolderActions = function (file) {
        $scope.actions = [];
        if (file.rights === 'WRITE') $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForFolder());
        $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForFolder());
    };

    setProjectActions = function (file) {
        $scope.actions = [];
        if (false) $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForProject());
        $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForProject());
    };

    setFileActions = function (file) {
        $scope.actions = [];
        if (file.rights === 'WRITE') $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForFile(file));
        $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForFile(file));
    };

    setNoTargetActions = function () {
        $scope.actions = [];
        if (true) $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsInFolder());
        $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsInFolder());
    };

    setNoTargetActionsProject = function () {
        $scope.actions = [];
        if (true) $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsInLobby());
        $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsInLobby());
    };

    $scope.execute = function (action) {
        action.execute(GlobalService.selectedFile);
    };
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
            property: "="
        },
        restrict: "E",
        template: propertyTemplate,
    };
});