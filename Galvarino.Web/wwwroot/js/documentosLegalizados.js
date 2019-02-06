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

            $.ajax({
                type: "GET",
                url: `/api/wf/v1/obtener-expediente/${expedientex.folioCredito}`
            }).done(function (data) {

                expedientex.obtenido = data;

                let index = _ingresados.findIndex(function (expedientey) {
                    return expedientey.folioCredito == expedientex.folioCredito
                });

                if (index > -1 && typeof _ingresados[index] != 'undefined' && expedientex.folioCredito === _ingresados[index].folioCredito) {
                    if (_ingresados[index].documentos.indexOf(expedientex.codigoTipoDocumento) < 0) {
                        _ingresados[index].documentos.push(expedientex.codigoTipoDocumento)
                    }
                } else {
                    if (expedientex.obtenido.id > 0) {
                        _ingresados.push(expedientex);
                    }
                }
                metodos.render();
            }).fail(function (errMsg) {
                $.niftyNoty({
                    type: "danger",
                    container: "floating",
                    title: "Suceso Erroneo",
                    message: "Error al Pistolear:  " + $("#folio-shot").val() + "<br/><small>Puede que el documento este duplicado o no disponible para esta nómina</small>",
                    closeBtn: true,
                    timer: 5000
                });
            })
        }
        $("#folio-shot").val("");
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
            let html = `<div class="btn-group dropdown mar-rgt">
                <button class="btn ${clasePrincipal} dropdown-toggle dropdown-toggle-icon" data-toggle="dropdown" type="button" aria-expanded="false">
                    ${exp.folioCredito} <i class="dropdown-caret"></i>
                </button>
                <ul class="dropdown-menu" style="">
                    ${internos}
                </ul>
            </div>`;

            $('.contenedor-folios').append(html);
        });


    },
    avanzarWf: function () {

        let notShotedFlag = false;
        let foliosEnvio = _ingresados.map(function (expediente) {

            notShotedFlag = expediente.documentos.length < expediente.obtenido.documentos.length;
            if (notShotedFlag) {
                return false;
            }

            return {
                folioCredito: expediente.folioCredito,
                marca: $('input[name=TipoMarca]:checked').val()
            };
        });

        if (notShotedFlag) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Validación de Negocio!",
                message: "Ups. Debes tener todos los documentos pistoleados para poder procesar.",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }

        if (foliosEnvio.length === 0) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Validación de Negocio!",
                message: "Ups. Debes agregar Expedientes para poder procesar.",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }
        //console.log(foliosEnvio);
        
        $.ajax({
            type: "POST",
            url: `/api/wf/v1/ingreso-expedientes-legalizados`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Ingreso de Expedientes Especiales",
                message: "Los expedientes fueron marcados con exito. <br/><small>Ahora los puedes enviar en la nomina para Of. de Partes.</small>",
                closeBtn: true,
                timer: 3000,
                onHidden: function () {
                    location.href = '/wf/v1/mis-solicitudes';
                }
            });
        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Ingreso de Expedientes ya Legalizados",
                message: "Tarea No Finalizada, contacte a Soporte! <small></small>",
                closeBtn: true,
                timer: 5000
            });
        }).always(function () {
            _ingresados = [];
        });
   
        
    }
};

$(function () {
    
    $('#form-pistoleo-detalle').on('submit', function (event) {
        event.preventDefault();
        metodos.disparo();
    });

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

})