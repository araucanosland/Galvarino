
window._ingresados = [];

const eventos = {
    pistolaDisparada: "snapshot.events.galvarino"
}
const metodos = {
    disparo: function(){
        var codigoTipoDocumento = $("#folio-shot").val().substring(0, 3);
        const expedientex = {
            codigoTipoDocumento,
            folioCredito: $("#folio-shot").val().substring(3, 15),
            rutAfiliado: $("#folio-shot").val().substring(15, 16),
            montoCredito: $("#folio-shot").val().substring(16, 17),
            completado: false,
            documentos: [codigoTipoDocumento],
            obtenido: {}
        }

        var index = _ingresados.findIndex(function (x) {
            return expedientey.folioCredito == expedientex.folioCredito
        });


        if (index > -1 && typeof _ingresados[index] != 'undefined' && _ingresados[index].documentos.indexOf(codigoTipoDocumento) > -1) {

            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Suceso Erroneo",
                message: "Error al Pistolear",
                closeBtn: true,
                timer: 5000
            });
        } else {
            $(document).trigger(eventos.pistolaDisparada, expedientex);
        }
    }
}

function formatoFecha(valor) {
    console.log(valor)
    return valor.toFecha(); //.toChileanDateString();
}

$(function(){

    $(document).on(eventos.pistolaDisparada, function (event, params) {
        
        let expediente = params;

        $.ajax({
            type: "GET",
            url: `/api/wf/v1/obtener-expediente/${expediente.folioCredito}`,
            contentType: "application/json; charset=utf-8"

        }).done(function (data) {
            expediente.obtenido=data;
            _ingresados.push(expediente);

        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Suceso Erroneo",
                message: "Error al Pistolear",
                closeBtn: true,
                timer: 5000
            });
        })

    });

    $("#modal-pistoleo").on("shown.bs.modal", function () {
        $("#folio-shot").focus();
    });

    $("#modal-pistoleo").on("hidden.bs.modal", function () {
        $("#folio-shot").val("");
    });

    $("#folio-shot").on("keydown", function(event){
        
        if ($.inArray(event.keyCode, [13, 8]) > -1){
            event.preventDefault();
        }

        if (parseInt($(this).val().length) === parseInt($(this).prop("maxlength"))) {
            metodos.disparo();
        }
    });

    $("#frm-pistoleo").on("submit", function(event){
        event.preventDefault()
        metodos.disparo();
    })

    $("#btn-generar-generico").on("click", function(){
        console.log('Generando')
    })
        
});