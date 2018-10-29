
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
            return x.folioCredito == expedientex.folioCredito
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
    },
    render: function(){

        $('.contenedor-folios').html("");
        $.each(_ingresados, function(i, exp){
            
            let internos = ``;

            $.each(exp.obtenido.documentos, function(i, doc){
                let calssE = exp.documentos.indexOf(doc.codificacion) > -1 ? "glyphicon-ok" : "glyphicon-remove";
                internos += `<li><a href="#">${enumTipoDocumentos[doc.tipoDocumento]} <i class="glyphicon ${calssE}" /></a></li>`
            });
            
            let clasePrincipal = exp.obtenido.documentos.length === exp.documentos.length  ? 'btn-success' : 'btn-warning';
            let html = `<div class="btn-group dropdown mar-rgt">
                <button class="btn ${clasePrincipal} dropdown-toggle dropdown-toggle-icon" data-toggle="dropdown" type="button" aria-expanded="false">
                    ${exp.folioCredito} <i class="dropdown-caret"></i>
                </button>
                <ul class="dropdown-menu" style="">
                    ${internos}
                </ul>
            </div>`;

            $('.contenedor-folios').append(html)
        })
            

    },
    avanzarWf: function(){
        let foliosEnvio = [];
        $.each(_ingresados, function (i, exp) {
            if(exp.obtenido.documentos.length === exp.documentos.length)
            {
                foliosEnvio.push({
                    FolioCredito: exp.folioCredito
                });
            }
        });

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/envio-a-notaria`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Notaría",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden(){
                    location.href = "/wf/v1/mis-solicitudes";
                    window.open(`/salidas/pdf/detalle-pack-notaria/${data.codigoSeguimiento}`, "_blank");
                }
            });
            
        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Avance Tareas",
                message: "Tarea No Finalizada, contacte a Soporte!",
                closeBtn: true,
                timer: 5000
            });

        }).always(function () {
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}



$(function(){

    $(document).on(eventos.pistolaDisparada, function (event, params) {
        
        let expediente = params;

        $.ajax({
            type: "GET",
            url: `/api/wf/v1/obtener-expediente/${expediente.folioCredito}`
        }).done(function (data) {

            expediente.obtenido = data;

            let index = _ingresados.findIndex(function (expedientey) {
                return expedientey.folioCredito == expediente.folioCredito
            });

            if (index > -1 && typeof _ingresados[index] != 'undefined' && expediente.folioCredito === _ingresados[index].folioCredito) {
                if (_ingresados[index].documentos.indexOf(expediente.codigoTipoDocumento) < 0) {
                    _ingresados[index].documentos.push(expediente.codigoTipoDocumento)
                }
            } else {
                if (expediente.obtenido.id > 0) {
                    _ingresados.push(expediente);
                }
            }

            $("#folio-shot").val("");
            metodos.render();

        }).fail(function (errMsg) {
            console.log(errMsg)
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Suceso Erroneo",
                message: "Error al Pistolear:  " + $("#folio-shot").val(),
                closeBtn: true,
                timer: 5000
            });

            $("#folio-shot").val("");
        })

    });

    $("#modal-pistoleo").on("shown.bs.modal", function () {
        $("#folio-shot").focus();
    });

    $("#modal-pistoleo").on("hidden.bs.modal", function () {
        $("#folio-shot").val("");
    });

    $("#frm-pistoleo").on("submit", function(event){
        event.preventDefault()
        metodos.disparo();
    });

    $("#btn-generar-generico").on("click", function(){
        console.log('Generando')
        metodos.avanzarWf();
    });
        
});