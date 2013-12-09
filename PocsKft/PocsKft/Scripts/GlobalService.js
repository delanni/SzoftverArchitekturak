
HBMAIN.factory("GlobalService", function (Communicator) {
    var returnObject = {};
    var ACTIONS = {
        MAKEGROUP:{
            name: "Create a new group",
            type: "group",
            execute: function () {
                Communicator.createGroup();
            }
        },
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
                returnObject.currentPath = url;
                window.location.pathname = url;
                //Communicator.notifyFS();
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
                Communicator.uploadFile(file.filePath + file.fileName);
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
            type: "unlock",
            execute: function () {
                var file = returnObject.selectedFile;
                Communicator.tryLock(file);
            }
        },
        UNLOCK: {
            name: "Release the lock",
            type: "lock",
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
        return [ACTIONS.CREATEPROJECT, ACTIONS.MAKEGROUP];
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
        var baseActions = [ACTIONS.EDIT, ACTIONS.SAVE];
        if (file.lockStatus === 'UNLOCKED') {
            return [ACTIONS.LOCK].concat(baseActions);
        } else if (file.lockStatus === 'LOCKED') {
            return [ACTIONS.LOCKED];
        } else {
            return [ACTIONS.UNLOCK, ACTIONS.UPDATE, ACTIONS.REVERT, ACTIONS.DELETE].concat(baseActions);
        }
    };
    ACTIONS.getReadActionsForFile = function (file) {
        return [ACTIONS.DOWNLOAD];
    };

    returnObject.ACTIONS = ACTIONS;

    return returnObject;
});