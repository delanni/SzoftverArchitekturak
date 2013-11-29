var FILESTATES = {
    LOCKED: "LOCKED",
    OPEN: "OPEN",
    ARCHIVED: "ARCHIVED"
};

var File = (function () {
    function _File(opts) {
        opts = opts || {};

        this.isRealFile = opts.isRealFile === false ? false : true;

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

        this.properties = opts.properties || [];
    }

    /**
    *   Tells if the file in question is a real file or a folder.
    **/
    _File.__defineGetter__("isRealFile", function () { return this.isRealFile; });

    /**
    *  Tells the state of the given file. States are as follows:
    *  LOCKED: this file is not open for modification
    *  OPEN: this file is open for modification
    *  ARCHIVED: this file has been deleted, the file is only an archived version
    **/
    _File.__defineGetter__("state", function () { return this.state; });


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
    * Getter for the projectName property
    **/
    _File.__defineGetter__("projectName", function () { return this.projectName; });

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