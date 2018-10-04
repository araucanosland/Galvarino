Number.prototype.toMoney = function (decimals=0, decimal_sep, thousands_sep) {
    var n = this,
        c = isNaN(decimals) ? 0 : Math.abs(decimals),
        d = decimal_sep || ',',
        t = (typeof thousands_sep === 'undefined') ? '.' : thousands_sep,
        sign = (n < 0) ? '-' : '',
        i = parseInt(n = Math.abs(n).toFixed(c)) + '',
        j = ((j = i.length) > 3) ? j % 3 : 0;
    return sign + (j ? i.substr(0, j) + t : '') + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : '');
};

Date.prototype.monthDays = function () {
    var d = new Date(this.getFullYear(), this.getMonth() + 1, 0);
    return d.getDate();
};

Date.prototype.monthDaysLeft = function () {

    var d = new Date(this.getFullYear(), this.getMonth() + 1, 0);
    return d.getDate() - this.getDate();
};

Date.prototype.toChileanDateString = function () {
    var month = String(this.getMonth() + 1);
    var day = String(this.getDate());
    const year = String(this.getFullYear());

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return day + '-' + month + '-' + year;
};

Date.prototype.toChileanDateTimeString = function () {
    var month = String(this.getMonth() + 1);
    var day = String(this.getDate());
    const year = String(this.getFullYear());

    var hour = String(this.getUTCHours());
    var minute = String(this.getMinutes());

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return day + '-' + month + '-' + year + ' ' + hour + ':' + minute;
};

String.prototype.toFechaHora = function () {
    var x = this.split("T")
    var fec = x[0]
    var hor = x[1]
    var y = fec.split("-")
    return y[2] + '-' + y[1] + '-' + y[0] + ' ' + hor;

};

String.prototype.toFecha = function () {

    if (this == "N/A")
        return this;

    var x = this.split("T")
    var fec = x[0]
    var y = fec.split("-")
    return y[2] + '-' + y[1] + '-' + y[0];
};

String.prototype.addSlashes = function () {
    //return this.replace(/[\\"']/g, '\\$&').replace(/\u0000/g, '\\0');
    return this.replace(/\'/g, '')
};

String.prototype.OrdenaNombre = function () {


    if (this.indexOf(",") > -1) {
        var EjecName = this.split(',');
        var EjecApellidos = EjecName[0];
        var EjecNombres = EjecName[1];
        var EjecNN = EjecNombres.trim().split(" ")
        var EjecAP = EjecApellidos.trim().split(" ")

        return EjecNN[0] + ' ' + EjecAP[0];
    } else {
        return this;
    }
};



(function ($) {
    $.fn.serializeFormJSON = function () {

        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name]) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };
})(jQuery);