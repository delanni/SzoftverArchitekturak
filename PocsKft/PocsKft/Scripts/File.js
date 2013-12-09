var LOCKSTATUS = {
    LOCKED: "LOCKED",
    UNLOCKED: "UNLOCKED",
    UNDERCONTROL: "UNDERCONTROL"
};

var RIGHTS = {
    WRITE: "WRITE",
    READ: "READ"
}

var File = (function () {
    function _File(opts) {
        opts = opts || {};

        this.isRealFile = opts.isRealFile === false ? false : true;

        this.fileName = opts.fileName || "";

        this.filePath = opts.filePath || "/";

        this.description = opts.description || "";

        if (opts.creationDate && opts.creationDate.match(/-?\d+/)) {
            this.creationDate = new Date(+opts.creationDate.match(/-?\d+/)[0]);
        } else {
            this.creationDate = opts.creationDate || new Date();
        }

        if (opts.lastModificationDate && opts.lastModificationDate.match(/-?\d+/)) {
            this.lastModificationDate = new Date(+opts.lastModificationDate.match(/-?\d+/)[0]);
        } else {
            this.lastModificationDate = this.creationDate || new Date();
        }

        this.lastModifierName = opts.lastModifierName || "unknown";

        this.lockStatus = opts.lockStatus || LOCKSTATUS.UNLOCKED;

        this.rights = opts.rights || RIGHTS.WRITE;

        if (this.filePath.lastIndexOf("/") !== (this.filePath.length - 1)) {
            this.filePath += "/";
        }

        if (opts.properties) {
            try{
                this.properties = JSON.parse(opts.properties);
            } catch (e) {
                this.properties = [];
            }
        } else {
            this.properties = [];
        }

        if (opts.versions) {
            this.versions = opts.versions.map(function (e) { return new Version(e); });
        } else {
            this.versions = [];
        }
    }

    /**
    * Getter and setter for the description property.
    * 
    **/
    _File.__defineGetter__("description", function () { return this.description; });
    _File.__defineSetter__("description", function (value) { this.description = value });

    /**
    *   Tells if the file in question is a real file or a folder.
    **/
    _File.__defineGetter__("isRealFile", function () { return this.isRealFile; });

    /**
    *  Tells the lock state of the given file. States are as follows:
    *  LOCKED: this file is not open for modification
    *  UNLOCKED: this file is open for modification
    *  UNAUTHORIZED: the current user has no right to lock the file
    *  UNDERCONTROL: this file is under the lock of the current user
    **/
    _File.__defineGetter__("lockStatus", function () { return this.lockStatus; });
    _File.__defineSetter__("lockStatus", function (value) { this.lockStatus = value; });


    /**
    * Getter and setter for the fileName property.
    **/
    _File.__defineGetter__("fileName", function () { return this.fileName; });
    _File.__defineSetter__("fileName", function (value) { this.fileName = value });

    /**
    * Getter and setter for the filePath property.
    **/
    _File.__defineGetter__("filePath", function () { return this.filePath; });
    _File.__defineSetter__("filePath", function (value) { this.filePath = value; });

    /**
    * Getter and setter for the lastModifierName property.
    * This is a string value with the name of the user who has created or lastly modified the file.
    **/
    _File.__defineGetter__("lastModifierName", function () { return this.lastModifierName; });
    _File.__defineSetter__("lastModifierName", function (value) { this.lastModifierName = value; });

    /**
    * Getter for the creationDate property
    **/
    _File.__defineGetter__("creationDate", function () { return this.creationDate; });

    /**
    * Getter and setter for the lastModificationDate property.
    * 
    **/
    _File.__defineGetter__("lastModificationDate", function () { return this.lastModificationDate; });
    _File.__defineSetter__("lastModificationDate", function (value) { this.lastModificationDate = value });

    /**
    * Getter and setter for the properties property.
    * The properties are an array of instances of the class Property.
    **/
    _File.__defineGetter__("properties", function () { return this.properties; });
    _File.__defineSetter__("properties", function (value) { this.properties = value });

    /**
    * Getter and setter for the right property.
    * 
    **/
    _File.__defineGetter__("rights", function () { return this.rights; });
    _File.__defineSetter__("rights", function (value) { this.rights = value });

    /**
    * Gets the path to the grandparent of the file
    **/
    _File.__defineGetter__("parentPath", function () {
        var parentPath = this.filePath.split("");
        parentPath.pop();
        parentPath = parentPath.join("").substring(0, parentPath.lastIndexOf("/"));
        return parentPath;
    });

    return _File;
})();