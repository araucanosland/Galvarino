window._ingresados = [];

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

        if (_ingresados[index].pistoleado.indexOf(codigoTipoDocumento) == -1){
            _ingresados[index].pistoleado.push(codigoTipoDocumento);
        }else{
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Operacion Expediente",
                message: "Documento ya pistoleado",
                closeBtn: true,
                timer: 5000
            });
        }
        $("#folio-shot").val("");
        this.render();
    },
    render: function () {

        $('.contenedor-folios').html("");
        $.each(_ingresados, function (i, exp) {

            let internos = ``;

            $.each(exp.obtenido.documentos, function (i, doc) {
                let calssE = exp.pistoleado.indexOf(doc.codificacion) > -1 ? "glyphicon-ok" : "glyphicon-remove";
                internos += `<li><a href="#">${enumTipoDocumentos[doc.tipoDocumento]} <i class="glyphicon ${calssE}" /></a></li>`
            });

            let clasePrincipal = exp.pistoleado.length > 0 ? exp.pistoleado.length === exp.obtenido.documentos.length ? 'btn-success' : 'btn-warning' : 'btn-danger';
            let html = `<div class="btn-group dropdown mar-rgt mar-top">
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
        let foliosEnvio = _ingresados.map(function(expediente){
            return {
                FolioCredito: expediente.folioCredito,
                DocumentosPistoleados: expediente.pistoleado,
                Faltante: expediente.obtenido.documentos.length != expediente.pistoleado.length
            }
        });

        let folioValija = $('#folioValijia').text();

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/apertura-valija/${folioValija}`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
               

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Apertura de Valija",
                message: "Valija: " + folioValija + " Aperturada.<br/><small>Estamos actualizando el estado de la valija..</small>",
                closeBtn: true,
                timer: 5000
            });

            $("#modal-pistoleo").modal('hide');

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
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}

function nroExpedientes(val, row, inc) {
    return row.expedientes.length;
}

function linkFolio(val, row, inc) {
    return `<button class="btn btn-link" data-target="#modal-pistoleo" data-toggle="modal" data-folio="${val}" >${val}</button>`;
}


$(function(){


    $("#modal-pistoleo").on("shown.bs.modal", function (event) {
        $("#folio-shot").focus();
    });

    $("#modal-pistoleo").on("show.bs.modal", function (event) {

        const folioValija = $(event.relatedTarget).data("folio");
        $('#folioValijia').text(folioValija);
         $.ajax({
             type: "GET",
             url: `/api/app/v1/listar-expedientes-valija/${folioValija}`
         }).done(function (data) {

            _ingresados = data.map(function(expediente){
                return {
                    codigoTipoDocumento: "",
                    folioCredito: expediente.credito.folioCredito,
                    rutAfiliado: expediente.credito.rutCliente,
                    montoCredito: expediente.credito.montoCredito,
                    completado: false,
                    documentos: [],
                    obtenido: expediente,
                    pistoleado: []
                };
            });

            metodos.render();

         }).fail(function (errMsg) {
             $.niftyNoty({
                 type: "danger",
                 container: "floating",
                 title: "Suceso Erroneo",
                 message: "Error al Pistolear",
                 closeBtn: true,
                 timer: 5000
             });
         });
    });

    $("#modal-pistoleo").on("hidden.bs.modal", function () {
        $("#folio-shot").val("");
        $('#folioValijia').text("");
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