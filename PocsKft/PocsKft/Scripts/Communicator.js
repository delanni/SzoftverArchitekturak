﻿/**
* The communicator Service is responsible for handling UI-to-server communication
* providing an api with functions for shared use
*
**/
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
                        error: function (e) { alert("Error during folder creation:\n" + JSON.stringify(e)) },
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
                        error: function (e) { alert("Error during project creation:\n" + JSON.stringify(e)) },
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
                deferred.reject(false); alert("Error during deleting resource:\n" + JSON.stringify(e));
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
                    var versionNum = +$(this).find("#versionNumber").val();
                    if (!confirm("Do you really want to revert the following :\n"
                    + fileUrl + ", to version " + versionNum)) { return; }
                    $.ajax({
                        type: "DELETE",
                        url: fileUrl,
                        success: function () {
                            $("#revertDialog").dialog("close");
                            notifyFS();
                        },
                        error: function (e) { alert("Error during reverting:\n" + JSON.stringify(e)) },
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
        var filePath = "";
        if (file.isProject)
            filePath = "/"+file.projectName+"/";
        else if (file.isRealFile)
            filePath = (file.filePath + file.fileName);
        else 
            filePath = (file.filePath + file.fileName) + "/";

        $.ajax({
            url: filePath,  //Server side action to process
            type: 'POST',
            dataType: 'json',
            //Ajax events
            beforeSend: function () { },
            success: function () {
                deferred.resolve();
                notifyFS();
            },
            error: function (e) {
                alert("Error updating file properties:\n" + JSON.stringify(e))
                deferred.reject();
            },
            // Form data
            data: { data: JSON.stringify(data) }
        });
    };

    var downloadFile = function (file) {
        window.open("/Download/" + file.filePath + file.fileName);
    };

    var tryLockFile = function (file) {
        var deferred = $q.defer();
        var filePath = file.filePath + file.fileName;
        $.ajax({
            url: filePath,  //Server side action to process
            type: 'POST',
            dataType: 'json',
            //Ajax events
            beforeSend: function () { },
            success: function () {
                deferred.resolve();
                notifyFS();
            },
            error: function (e) {
                alert("Error locking file:\n" + JSON.stringify(e))
                deferred.reject();
            },
            // Form data
            data: { "lock": filePath }
        });
    }

    var unlockFile = function (file) {
        var deferred = $q.defer();
        var filePath = file.filePath + file.fileName;
        $.ajax({
            url: filePath,  //Server side action to process
            type: 'POST',
            dataType: 'json',
            //Ajax events
            beforeSend: function () { },
            success: function () {
                deferred.resolve();
                notifyFS();
            },
            error: function (e) {
                alert("Error unlocking file:\n" + JSON.stringify(e))
                deferred.reject();
            },
            // Form data
            data: { "unlock": filePath }
        });
    }


    return {
        listFolder: listAsync,
        revert:revert,
        delete: deleteResource,
        updateFile: uploadFile,
        uploadFile: uploadFile,
        updateMeta: updateFileProperties,
        download: downloadFile,
        tryLock: tryLockFile,
        unlock: unlockFile,
        createProject: createProject,
        createFolder: createFolder,
        notifyFS: notifyFS
    };
});