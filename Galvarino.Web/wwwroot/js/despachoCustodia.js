
const metodos = {
   
    avanzarWf: function () {

        let data = $('#tabla-generica').bootstrapTable('getData');
        let foliosEnvio = data.map(function (exp) {
            return {
                folioCredito: exp.credito.folioCredito
            }
        });


        $.ajax({
            type: "POST",
            url: `/api/wf/v1/despacho-a-custodia`,
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


            doc.setFontSize(16)
            doc.text(20, 100, 'Envío de Caja a Custodia')
            doc.setFontSize(14)
            doc.text(20, 115, 'Fecha: ' + new Date(data.fechaEnvio).toLocaleDateString())
            doc.addImage(canvas.toDataURL('image/jpeg', 1.0), 'JPEG', 250, 100, 170, 45)
            doc.text(20, 145, 'Cantidad Expedientes: ' + data.expedientes.length)


            doc.addPage()
            doc.setFontSize(14)
            doc.text(20, 20, 'Nómina envío a Custodia')
            doc.setFontSize(12)
            doc.text(20, 35, 'Fecha: ' + new Date(data.fechaEnvio).toLocaleDateString())
            doc.addImage(canvas.toDataURL('image/jpeg', 1.0), 'JPEG', 250, 5, 170, 45)
            //doc.text(20, 47, data.oficina.nombre)
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



$(function () {

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});