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

HBMAIN.factory("GlobalService", function (Communicator) {
    var returnObject = {};
    var ACTIONS = {
        REVERT: {
            name: "Revert file to an older version",
            type: "history",
            execute: function () {
                var file = returnObject.selectedFile;
                Communicator.revert(file);
            }
        },
        BACK: {
            name: "Go back one level",
            type: "unshare",
            execute: function () {
                var current = window.location.pathname;
                var levels = current.split("/");
                levels.pop(); levels.pop();
                var url = levels.join("/") + "/";
                window.location.pathname = url;
            }
        },
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
        return [];
    };
    ACTIONS.getWriteActionsInFolder = function () {
        return [ACTIONS.CREATEFOLDER, ACTIONS.UPLOAD];
    };
    ACTIONS.getReadActionsInFolder = function () {
        return [ACTIONS.BACK];
    };
    ACTIONS.getWriteActionsForFolder = function () {
        return [ACTIONS.EDIT, ACTIONS.SAVE, ACTIONS.DELETE];
    };
    ACTIONS.getReadActionsForFolder = function () {
        return [];
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

    var createFolder = function (currentPath) {
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
                        error: function (e) { alert("Error:\n" + JSON.stringify(e)) },
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

    var createProject = function () {
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
                        error: function (e) { alert("Error:\n" + JSON.stringify(e)) },
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

    var uploadFile = function (filePath) {
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

                    var form = $(this).find("form");
                    form.attr("action", filePath);
                    form.submit();
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    };

    var deleteResource = function (file) {
        var deferred = $q.defer();
        var resourceUrl = file.isProject ? file.projectName : (file.filePath + file.fileName);
        if (!file.isRealFile) resourceUrl += "/";
        if (!confirm("Do you really want to delete the following :\n" + resourceUrl + "?")) { return; }
        HBMAIN.setLoading(false);
        $.ajax({
            type: "DELETE",
            url: resourceUrl,
            success: function () {
                deferred.resolve(true);
                notifyFS();
            },
            error: function (e) {
                deferred.reject(false); alert("Error:\n" + JSON.stringify(e));
            },
        });

        return deferred.promise;
    };

    var revert = function (file) {
        $("#revertDialog").dialog({
            resizable: false,
            modal: true,
            buttons: {
                "Revert!": function () {
                    var fileUrl = file.pathName + file.fileName;
                    $.ajax({
                        type: "POST",
                        url: fileUrl,
                        success: function () {
                            $("#revertDialog").dialog("close");
                            notifyFS();
                        },
                        error: function (e) { alert("Error:\n" + JSON.stringify(e)) },
                    });
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });
    }

    var updateFileProperties = function (file) {
        var deferred = $q.defer();
        var data = file;
        var filePath = file.filePath + file.fileName;
        $.ajax({
            url: filePath,  //Server side action to process
            type: 'POST',
            dataType:'json',
            //Ajax events
            beforeSend: function () { },
            success: function () {
                deferred.resolve();
                $("#fileUploadDialog").dialog("close");
                notifyFS();
            },
            error: function (e) {
                alert("Error:\n" + JSON.stringify(e))
                deferred.reject();
            },
            // Form data
            data: { data: JSON.stringify(data) }
        });
    };

    var downloadFile = function (file) {
        window.open("/Download/" + file.filePath + file.fileName);
    };

    return {
        listFolder: listAsync,
        delete: deleteResource,
        updateFile: uploadFile,
        uploadFile: uploadFile,
        updateMeta: updateFileProperties,
        download: downloadFile,
        tryLock: angular.noop,
        unlock: angular.noop,
        createProject: createProject,
        createFolder: createFolder
    };
});

HBMAIN.controller("BrowserController", ["$scope", "$rootScope", "Communicator", "GlobalService",
    function ($scope, $rootScope, Communicator, GlobalService) {
        $scope.reload = function () {
            $scope.currentPath = window.location.pathname || "/";
            GlobalService.currentPath = $scope.currentPath;
            $rootScope.$broadcast("pathChanged", $scope.currentPath);
            window.document.title = decodeURI($scope.currentPath);
            Communicator.listFolder($scope, $scope.currentPath).then(function (data) {
                $scope.files = data.map(function (e) {
                    if (e.isProject) return new Project(e); else return new File(e);
                });
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

        $scope.addNewProperty = function () {
            var file = $scope.global.selectedFile;
            if (file.properties && file.properties[file.properties.length-1]) {
                if (file.properties[file.properties.length - 1].propName == file.properties[file.properties.length - 1].propValue && file.properties[file.properties.length - 1].propValue==='') {
                    return;
                }
            }
            file.properties.push(new Property());
            setTimeout(function () {
                $(".edit").each(function (i, e) {
                    if ($(e).css("display") === 'none')
                        $(e).slideDown(200);
                });
            }, 300);
        };

        $scope.save = function () {
            GlobalService.ACTIONS.SAVE.execute();
        };

        $scope.discard = function () {
            $rootScope.$broadcast("fileSelectionChanged");
        };
    }]);

HBMAIN.controller("ActionBarController", ["$scope", "Communicator", "GlobalService", function ($scope, Communicator, GlobalService) {
    $scope.global = GlobalService;

    ACTIONS = GlobalService.ACTIONS;

    $scope.$on("fileSelectionChanged", function () {
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
    //var propertyTemplate = "<div class='property'>\
    //                        <div class='propname' ng-bind='property.propName' />\
    //                        <input class='edit propname' ng-model='property.propName'/>\
    //                        <div class='propvalue' ng-bind='property.propValue' />\
    //                        <input class='edit propvalue' ng-model='property.propValue' />\
    //                        </div>";
    var propertyTemplate = "<div class='property'>\
                            <div class='propname' style='font-weight:bold'>{{property.propName}} : {{property.propValue}}</div>\
                            <label class='edit'>Property name:</label>\
                            <input class='edit propname' ng-model='property.propName'/>\
                            <label class='edit'>Property value:</label>\
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