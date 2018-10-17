
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



            JsBarcode("#barcode", data.codigoSeguimiento);

            var canvas = document.getElementById('barcode');

            $("#barcode").hide();
            let doc = new jsPDF({
                unit: 'px',
                format: 'letter'
            });


            doc.setFontSize(14)
            doc.text(20, 20, 'Nómina de envío a notaría')
            doc.setFontSize(12)
            doc.text(20, 35, 'Fecha: ' + new Date(data.fechaEnvio).toLocaleDateString())
            doc.text(125, 35, data.notariaEnvio.nombre)
            doc.addImage(canvas.toDataURL('image/jpeg', 1.0), 'JPEG', 250, 5, 170, 45)

            doc.text(20, 47, data.oficina.nombre)
            doc.text(125, 47, 'Cantidad Expedientes: ' + data.expedientes.length)

            doc.text(20, 70, '#')
            doc.text(35, 70, 'Folio')
            doc.text(150, 70, 'Rut')
            doc.text(250, 70, 'Estado')
            doc.text(350, 70, 'Fecha Otorg.')
            doc.line(20, 72, 420, 73)

            //let partida = 85;
            doc.setLineWidth(0.5)
            let contadorC = 1;
            let pagina = 1;
            let controladorHeader = false;
            let partida2 = 35;
            let index = 0;
            for (let partida = 85; partida <= (70 + data.expedientes.length * 15); partida = partida + 15) {

                if (pagina > 1) {

                    if (controladorHeader) {
                        doc.text(20, 20, '#')
                        doc.text(35, 20, 'Folio')
                        doc.text(150, 20, 'Rut')
                        doc.text(250, 20, 'Estado')
                        doc.text(350, 20, 'Fecha Otorg.')
                        doc.line(20, 22, 420, 22)
                        partida2 = 35;
                    }


                    doc.text(20, partida2, contadorC.toString())
                    doc.text(35, partida2, data.expedientes[index].credito.folioCredito)
                    doc.text(150, partida2, data.expedientes[index].credito.rutCliente)
                    doc.text(250, partida2, enumTipoCreditos[data.expedientes[index].credito.tipoCredito])
                    doc.text(350, partida2, new Date(data.expedientes[index].credito.fechaDesembolso).toLocaleDateString())

                    partida2 = partida2 + 15;
                } else {
                    doc.text(20, partida, contadorC.toString())
                    doc.text(35, partida, data.expedientes[index].credito.folioCredito)
                    doc.text(150, partida, data.expedientes[index].credito.rutCliente)
                    doc.text(250, partida, enumTipoCreditos[data.expedientes[index].credito.tipoCredito])
                    doc.text(350, partida, new Date(data.expedientes[index].credito.fechaDesembolso).toLocaleDateString())
                }

                if (contadorC % 25 == 0) {
                    doc.addPage()
                    pagina++;
                    controladorHeader = true;
                } else {
                    controladorHeader = false;
                }
                contadorC++;
                index++;
            }

            doc.text(60, 550, 'Firma Entrega Conforme')
            doc.text(280, 550, 'Firma Recibí Conforme')
            doc.autoPrint();
            doc.output('dataurlnewwindow');



            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Avance Tareas",
                message: "Tarea Finalizada!!",
                closeBtn: true,
                timer: 5000
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