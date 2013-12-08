var Version = (function () {
    function _Version(opts) {
        this.versionNumber = opts.versionNumber || {};
        if (opts.date && opts.date.match(/-?\d+/)) {
            this.date = new Date(+opts.date.match(/-?\d+/)[0]);
            this.date = this.date.toDateString();
        } else {
            this.date = "Unknown date";
        }
    }

    /**
    * Getter and setter for the versionNumber property.
    * 
    **/
    _Version.__defineGetter__("versionNumber", function () { return this.versionNumber; });
    _Version.__defineSetter__("versionNumber", function (value) { this.versionNumber = value });

    /**
    * Getter and setter for the date property.
    * 
    **/
    _Version.__defineGetter__("date", function () { return this.date; });
    _Version.__defineSetter__("date", function (value) { this.date = value });

    return _Version;
})();