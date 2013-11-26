var Folder = (function () {
    function _Folder(opts) {
        opts = opts || {};

        this.folderName = opts.folderName || "folder" + (Math.random() * 1000) | 0;
        this.folderPath = opts.folderPath || ".";
        this.creationDate = opts.creationDate || {};
        this.projectName = opts.projectName || "root";
        this.lastModifierName = opts.lastModifierName || "unknown";

        if (this.folderPath.lastIndexOf("/") !== (this.folderPath.length - 1)) {
            this.folderPath += "/";
        }
    }

    _Folder.__defineGetter__("folderName", function () { return this.folderName; });
    _Folder.__defineSetter__("folderName", function (value) { this.folderName = value });

    _Folder.__defineGetter__("folderPath", function () { return this.folderPath; });
    _Folder.__defineSetter__("folderPath", function (value) { this.folderPath = value });

    _Folder.__defineGetter__("lastModifierName", function () { return this.lastModifierName; });
    _Folder.__defineSetter__("lastModifierName", function (value) { this.lastModifierName = value; });

    _Folder.__defineGetter__("projectName", function () { return this.projectName; });
    _Folder.__defineGetter__("creationDate", function () { return this.creationDate; });

    return _Folder;
})();