var FILESTATES = {
    LOCKED: "locked",
    OPEN: "open",
    ARCHIVED: "archived"
};

var File = (function () {
    function _File(opts) {
        opts = opts || {};
        this.fileName = opts.fileName || "";
        this.filePath = opts.filePath || "./";
        this.creationDate = opts.creationDate || new Date();
        this.lastModificationDate = opts.lastModificationDate || this.creationDate;
        this.projectName = opts.projectName || "root";
        this.lastModifierName = opts.lastModifierName || "unknown";
        this.state = opts.state || FILESTATES.LOCKED;

        if (this.filePath.lastIndexOf("/") !== (this.filePath.length - 1)) {
            this.filePath += "/";
        }
    }

    _File.__defineGetter__("state", function () { return this.state; });

    _File.__defineGetter__("fileName", function () { return this.fileName; });
    _File.__defineSetter__("fileName", function (value) { this.fileName = value; });

    _File.__defineGetter__("filePath", function () { return this.filePath; });
    _File.__defineSetter__("filePath", function (value) { this.filePath = value; });

    _File.__defineGetter__("lastModifierName", function () { return this.lastModifierName; });
    _File.__defineSetter__("lastModifierName", function (value) { this.lastModifierName = value; });

    _File.__defineGetter__("projectName", function () { return this.projectName; });
    _File.__defineGetter__("creationDate", function () { return this.creationDate; });

    return _File;
})();