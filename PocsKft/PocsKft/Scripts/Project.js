var Project = (function () {
    function _Project(opts) {
        opts = opts || {};

        this.filePath = "/";
        this.isProject = true;

        this.projectName = opts.projectName || "";

        this.description = opts.description || "";

        if (opts.creationDate && opts.creationDate.match(/-?\d+/)) {
            this.creationDate = new Date(+opts.creationDate.match(/-?\d+/)[0]);
        } else {
            this.creationDate = new Date();
        }

        if (opts.properties) {
            try {
                this.properties = JSON.parse(opts.properties);
            } catch (e) {
                this.properties = [];
            }
        } else {
            this.properties = [];
        }

        this.ownerName = opts.ownerName || "Admin";
        this.rights = opts.rights || "READ";
    }

    /**
    * Getter and setter for the description property.
    * 
    **/
    _Project.__defineGetter__("description", function () { return this.description; });
    _Project.__defineSetter__("description", function (value) { this.description = value });

    /**
    * Getter and setter for the projectName property.
    **/
    _Project.__defineGetter__("projectName", function () { return this.projectName; });
    _Project.__defineSetter__("projectName", function (value) { this.projectName = value });

    /**
    * Getter and setter for the creationDate property.
    **/
    _Project.__defineGetter__("creationDate", function () { return this.creationDate; });
    _Project.__defineSetter__("creationDate", function (value) { this.creationDate = value });

    /**
    * Getter and setter for the ownerName property.
    **/
    _Project.__defineGetter__("ownerName", function () { return this.ownerName; });
    _Project.__defineSetter__("ownerName", function (value) { this.ownerName = value });

    _Project.__defineGetter__("isProject", function () { return this.isProject; });


    /**
    * Getter and setter for the rights property.
    **/
    _Project.__defineGetter__("rights", function () { return this.rights; });
    _Project.__defineSetter__("rights", function (value) { this.rights = value });


    /**
    * Getter and setter for the properties property.
    * The properties are an array of instances of the class Property.
    **/
    _Project.__defineGetter__("properties", function () { return this.properties; });
    _Project.__defineSetter__("properties", function (value) { this.properties = value });


    return _Project;
})();