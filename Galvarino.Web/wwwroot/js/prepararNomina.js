window._ingresados = [];

const eventos = {
    pistolaDisparada: "snapshot.events.galvarino"
}
const metodos = {
    disparo: function () {

        var codigoTipoDocumento = $("#folio-shot").val().substring(formatoFolios.codigo.inicio, formatoFolios.codigo.fin);
        const expedientex = {
            codigoTipoDocumento,
            folioCredito: $("#folio-shot").val().substring(formatoFolios.folioCredito.inicio, formatoFolios.folioCredito.fin),
            rutAfiliado: $("#folio-shot").val().substring(formatoFolios.rutAfiliado.inicio, formatoFolios.rutAfiliado.fin),
            completado: false,
            documentos: [codigoTipoDocumento],
            obtenido: {},
            pistoleado: []
        }


        var index = _ingresados.findIndex(function (x) {
            return x.folioCredito == expedientex.folioCredito
        });


        if (index > -1 && typeof _ingresados[index] != 'undefined' && _ingresados[index].documentos.indexOf(codigoTipoDocumento) > -1) {

            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Operacion Expediente",
                message: "Documento ya pistoleado",
                closeBtn: true,
                timer: 5000
            });
        } else {
            $(document).trigger(eventos.pistolaDisparada, expedientex);
        }
    },
    render: function () {

        $('.contenedor-folios').html("");
        $.each(_ingresados, function (i, exp) {

            let internos = ``;

            $.each(exp.obtenido.documentos, function (i, doc) {
                let calssE = exp.documentos.indexOf(doc.codificacion) > -1 ? "glyphicon-ok" : "glyphicon-remove";
                internos += `<li><a href="#">${enumTipoDocumentos[doc.tipoDocumento]} <i class="glyphicon ${calssE}" /></a></li>`
            });

            let clasePrincipal = exp.obtenido.documentos.length === exp.documentos.length ? 'btn-success' : 'btn-warning';
            let completado = exp.obtenido.documentos.length === exp.documentos.length ? true : false;
            exp.completado = completado;
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
    avanzarWf: function () {

        let _todosCompletados = true;
        let foliosEnvio = [];
        $.each(_ingresados, function (i, exp) {
           
            if (exp.completado == false) {
                _todosCompletados = false;
            }


            if (exp.obtenido.documentos.length === exp.documentos.length) {
               
                foliosEnvio.push({
                    FolioCredito: exp.folioCredito
                });
            }
        });

        if (_todosCompletados == false) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error Preparar Nómina",
                message: "Debe Pistolear Todos los Documentos",
                closeBtn: true,
                timer: 5000
            });
            return false;

        }


        if (foliosEnvio.length == 0) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error Preparar Nómina",
                message: "No puedes Generar una Nómina de Envío Sin Documentos",
                closeBtn: false,
                timer: 5000
            });
            return false;
        }




        $.ajax({
            type: "POST",
            url: `/api/wf/v1/preparar-nomina`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function () {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Notaría",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se cierra en 5 Seg. y te redirige a tus solicitudes.</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    location.href = '/wf/v1/mis-solicitudes';
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
            _ingresados = [];
            $("#modal-pistoleo").modal('hide');
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}



$(function () {

    $(document).on(eventos.pistolaDisparada, function (event, params) {

        let expediente = params;
        let etapa = $("#etapa-actual").val();
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/obtener-expediente/${expediente.folioCredito}/${etapa}`
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
                title: "Error al Pistolear:  " + $("#folio-shot").val(),
                message: errMsg.responseText,
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
        _ingresados = [];
        metodos.render();
    });

    $("#frm-pistoleo").on("submit", function (event) {
        event.preventDefault()
        metodos.disparo();
    });

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});