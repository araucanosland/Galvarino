
const metodos = {
    render: function (_ingresados = []) {
        $("#total-expedientes").html(`Expedientes pistoleados: <strong>${_ingresados.length}</strong>`);
        
        $('.contenedor-folios').html("");
        $.each(_ingresados, function (i, exp) {
            
            let clasePrincipal = exp.pistoleados === exp.total ? 'btn-success' : 'btn-warning';
            let html = `<div class="btn-group dropdown mar-rgt mar-btm">
                <button class="btn ${clasePrincipal}" type="button">
                    ${exp.folio}
                </button>
            </div>`;

            $('.contenedor-folios').append(html);
        });


    },
    avanzarWf: function (codigoCaja) {
        
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/${codigoCaja}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            /*$.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Custodia",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    window.open(`/salidas/pdf/detalle-caja-valorada/${codigoCaja}`, "_blank");
                }
            });*/
            //<div class="alert alert-info"><strong>Muchos Registos!</strong> Hay muchos registros en esta vista y no se vera por temas de rendimiento.</div>

            $('#modal-pistoleo').modal("hide");
            
            var message = $('<div>').addClass(`alert alert-warning mar-btm mensahe-${codigoCaja}`)
                .append($('<strong>').text('Procesando caja en segundo plano!'))
                .append(" ..Se está procesando la caja en segundo plano, cuando esté lista se habilitará el link para ver la nomina.");


            $('#message-placeholder').prepend(message);

        }).fail(function (errMsg) {


            console.log({
                errMsg
            })

            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Avance Tareas",
                message: "Tarea No Finalizada, contacte a Soporte!",
                closeBtn: true,
                timer: 5000
            });

        }).always(function () {
            //$('#tabla-generica').bootstrapTable('refresh');
        });
    },
    pistoleo: function (codigoCaja, folioDocumento) {
        //TODO: se debe enviar señal al servidor con documento agregandolo a una caja
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/caja-valorada/${codigoCaja}/agregar-documento/${folioDocumento}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            console.log({
                data
            })
            metodos.render(data);
            
        }).fail(function (errMsg) {
            console.log(errMsg);
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Error al generar Caja",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000
            });
        });
    },
    generarCaja: function () {
        //TODO: cuando se abre el modal se debe generar una caja automaticamente para llenarla con documentos
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/generar-caja-valorada`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            console.log(data);
            $('.titulo-genera-cajas').text(`Generando Caja Folio: ${data.caja.codigoSeguimiento}`);
            $('#codigo-caja-valorada').val(data.caja.codigoSeguimiento);
            metodos.render(data.documentos);
        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Error al generar Caja",
                message: "Hay un problema al generar la caja, favor comunicate con el desarrollador",
                closeBtn: true,
                timer: 5000
            });
        });
    }
};


//Eventos
$(function () {

    $("#btn-generar-generico").on("click", function () {
        var caja = $('#codigo-caja-valorada').val();
        metodos.avanzarWf(caja);
    });

    $('#modal-pistoleo').on('show.bs.modal', function () {
        metodos.generarCaja();
    });

    $("#frm-pistoleo").on("submit", function (event) {
        event.preventDefault();
        var folio = $('#folio-shot').val();
        var caja = $('#codigo-caja-valorada').val();
        metodos.pistoleo(caja, folio);

        $('#folio-shot').val("");
    });

});


//SignalR - Live connection

var connection = new signalR.HubConnectionBuilder().withUrl("/caja-cerrada-hub").build();

connection.on("NotificarCajaCerrada", function (codigo) {


    console.log({
        codigo,
        nombre: "NotificarCajaCerrada"
    });
    $(`.mensahe-${codigo}`).removeClass('alert-warning').addClass("alert-success").html("")
        .append($('<strong>').text('Caja Procesada!'))
        .append(`Ahora puedes ver la nomina de la caja en el siguiente link: <a class="btn-link" href="/salidas/pdf/detalle-caja-valorada/${codigo}" target="_blank">Caja: ${codigo}</a>.`);
});


connection.start().then(function () {
    console.log({
        connection
    });
}).catch(function (err) {
    return console.error(err.toString());
});