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

function soloNumeros(e) {
    tecla = (document.all) ? e.keyCode : e.which;

    //Tecla de retroceso para borrar, siempre la permite
    if (tecla == 8 || tecla == 13) {
        return true;
    }

    // Patron de entrada, en este caso solo acepta numeros
    patron = /[0-9]/;
    tecla_final = String.fromCharCode(tecla);
    return patron.test(tecla_final);
}



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

    
    $('.solo-numeros').on('keydown', function(e){
        return soloNumeros(e);
    })

    let $li = $(`a[href="${location.pathname}"]`).closest("li");
    let $ul = $li.closest("ul");
    $li.addClass("active-link")
    if ($ul.hasClass('collapse')) {
        $parentLi = $ul.closest("li");
        $parentLi.addClass("active-sub");
        $ul.addClass("in");
    }



    $('#form-busqueda-general').on('submit', function(e){
        e.preventDefault();
        var folioCredito = $('#search-input').val();
        location.href = `/busqueda/resultado-busqueda/${folioCredito}`;
    });
    
})(jQuery);

/*Cargando... */
$(document).ajaxStart(function () {
    var target = $("body"),
        panelOv = $('<div id="principal-loader" class="panel-overlay"></div>');
    target.addClass('panel-overlay-wrap'),
        icon = '<span class="panel-overlay-icon text-main"><i class="fa fa-refresh fa-spin fa-2x"></i></span>';
    target.data('overlayTemplate', '<div class="panel-overlay-content pad-all unselectable">' + icon + '<h4 class="panel-overlay-title"></h4><p></p></div>');

    panelOv.appendTo(target).html(target.data('overlayTemplate'));
});

$(document).ajaxStop(function () {
    $("body").removeClass('panel-overlay-wrap');
    $("#principal-loader").hide().remove();
});


/* Funciones especiales para Tablas */
function formatoFecha(valor) {
    return valor.toFecha();
}

function formatoTipoCredito(valor) {
    return enumTipoCreditos[valor];
}

function formatoListaDocumentos(val, row, inc)
{
    let strOut = row.documentos.map(function (val, idx) {
        return `<strong>${val.codificacion}</strong> - ${enumTipoDocumentos[val.tipoDocumento]}`
    });
    return strOut.join('<br />');
}
