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
    var returnObject = {};
    ACTIONS = {
        DELETE: {
            name: "Delete",
            type: "bin",
            execute: function () {
                file = returnObject.selectedFile;
                var promise = Communicator.delete(file);
                promise.then(function (success) {
                    HBMAIN.setLoading(false);
                }, function (notfailure) {
                    HBMAIN.setLoading(false);
                });
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
            execute: function () {
                var currentPath = returnObject.currentPath;
                Communicator.uploadFile(currentPath);
            }
        },
        UPDATE: {
            name: "Upload new version",
            type: "git_commit",
            execute: function () {
                var file = returnObject.selectedFile;
                Communicator.updateFile(file);
            }
        },
        DOWNLOAD: {
            name: "Download",
            type: "download",
            execute: function () {
                var file = returnObject.selectedFile;
                Communicator.download(file);
            }
        },
        LOCK: {
            name: "Try to acquire lock on the file",
            type: "lock",
            execute: function () {
                var file = returnObject.selectedFile;
                Communicator.tryLock(file);
            }
        },
        UNLOCK: {
            name: "Release the lock",
            type: "unlock",
            execute: function () {
                var file = returnObject.selectedFile;
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
            execute: function () {
                var file = returnObject.selectedFile;
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
                Communicator.createFolder(returnObject.currentPath);
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

    returnObject.ACTIONS = ACTIONS;

    return returnObject;
});

HBMAIN.factory("TestUser", function () {
    var user = new User({ name: "Alex" });
    return user;
});

HBMAIN.factory("Communicator", function ($http, $q, $rootScope) {
    var c = {};

    var notifyFS = function () {
        $rootScope.$broadcast("fileStructureChanged");
    }

    var listAsync = function (scope, folderName) {
        var deferred = $q.defer();
        var url = folderName;

        $http.get(url).success(function (data) {
            if (data && data.hasOwnProperty("length")) {
                deferred.resolve(data);
            } else if (!data) {
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

    var createFolderFake = function (currentPath) {
        $("#folderDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Create": function () {
                    var folderName = $("#folderNameBox").val();
                    var putUrl = currentPath + folderName;
                    $.ajax({
                        type: "PUT",
                        url: putUrl,
                        beforeSend: function () { },
                        success: function () { $("#folderDialog").dialog("close"); $("#folderNameBox").val(""); notifyFS(); },
                        error: function (e) { alert("Error during creating folder, please try again."); },
                        cache: false,
                        contentType: false,
                        processData: false
                    });
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    var createProjectFake = function () {
        $("#projectDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Create": function () {
                    var folderName = $("#projectNameBox").val();
                    var putUrl = "/" + folderName;
                    $.ajax({
                        type: "PUT",
                        url: putUrl,
                        beforeSend: function () { },
                        success: function () {
                            $("#projectDialog").dialog("close");
                            $("#projectNameBox").val("");
                            notifyFS();
                        },
                        error: function (e) { alert("Error during creating project, please try again."); },
                        cache: false,
                        contentType: false,
                        processData: false
                    });
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    var uploadFileFake = function (filePath) {
        $("#fileUploadDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Upload": function () {
                    var file = $(this).find("#file");
                    var progress = $(this).find("progress");
                    file.change(function (f) {
                        // validate for file
                    });
                    var progressHandlingFunction = function (event) {
                        if (event.lengthComputable) {
                            progress.attr({ value: event.loaded, max: event.total });
                        }
                    };

                    var formData = new FormData($('form')[0]);
                    $.ajax({
                        url: filePath,  //Server side action to process
                        type: 'POST',
                        xhr: function () {  // Custom XMLHttpRequest
                            var myXhr = $.ajaxSettings.xhr();
                            if (myXhr.upload) { // Check if upload property exists
                                myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // For handling the progress of the upload
                            }
                            return myXhr;
                        },
                        //Ajax events
                        beforeSend: function () { },
                        success: function () {
                            $("#fileUploadDialog").dialog("close");
                            notifyFS();
                        },
                        error: function (e) { alert("Error during uploading, please try again."); },
                        // Form data
                        data: formData,
                        //Options to tell jQuery not to process data or worry about content-type.
                        cache: false,
                        contentType: false,
                        processData: false
                    });
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    var deleteResourceFake = function (file) {
        var deferred = $q.defer();
        var resourceUrl = file.isProject ? file.projectName : (file.filePath + file.fileName);
        HBMAIN.setLoading(false);
        $.ajax({
            type: "DELETE",
            url: resourceUrl,
            success: function () {
                deferred.resolve(true);
                notifyFS();
            },
            error: function (e) {
                deferred.reject(false); alert("Error during creating folder, please try again.");
            },
        });

        return deferred.promise;
    };


    return {
        listFolder: listAsync,
        delete: deleteResourceFake,
        updateFile: uploadFileFake,
        uploadFile: uploadFileFake,
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
            $scope.currentPath = window.location.pathname || "/";
            GlobalService.currentPath = $scope.currentPath;
            $rootScope.$broadcast("pathChanged", $scope.currentPath);

            Communicator.listFolder($scope, $scope.currentPath).then(function (data) {
                $scope.files = data;
                $scope.clearSelections();
                HBMAIN.setLoading(false);
            });
        };

        $scope.$on("fileStructureChanged", function () {
            $scope.reload();
        });

        $scope.select = function (selected, event) {
            if (selected.selected && !selected.isRealFile) {
                delete GlobalService.selectedFile;
                $scope.currentPath = selected.filePath + (selected.fileName || selected.projectName) + "/";
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