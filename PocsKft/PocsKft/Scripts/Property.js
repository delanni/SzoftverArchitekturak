var Property = (function () {
    function _Property(opts) {
        if (arguments.length > 1) {
            opts = {
                propName: arguments[0],
                propValue: arguments[1]
            };
        }
        opts = opts || {};
        this.propName = opts.propName || "property";
        this.propValue = opts.propValue || {};
    }

    _Property.__defineGetter__("propName", function () { return this.propName; });
    _Property.__defineSetter__("propName", function (value) { this.propName = value });

    _Property.__defineGetter__("propValue", function () {
        if (typeof this.propValue === 'string') return this.propValue;
        else try{
            return JSON.stringify(this.propValue);
        } catch (ex) {
            return JSON.stringify(ex);
        }
    });
    _Property.__defineSetter__("propValue", function (value) { this.propValue = value });

    _Property.fromObject = function (propertyObject) {
        var propertyArray = [];
        for (var p in propertyObject) {
            propertyArray.push(new Property(p, propertyObject[p]));
        }
        return propertyArray;
    }

    return _Property;
})();