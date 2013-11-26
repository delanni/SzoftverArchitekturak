var User = (function () {

    function _User(opts) {
        opts = opts || {};
        this.name = opts.name || "Unnamed";
        this.email = opts.email || "an@ls.ex";
        this.picUrl = opts.picUrl || "not.jpg";
    }

    _User.prototype.getFriendlyName = function () {
        return this.name;
    };

    return _User;
})();