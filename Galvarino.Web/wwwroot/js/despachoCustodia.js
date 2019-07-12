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
                .append($('<strong>').text('La caja aparecera mañana!!'))
                .append(" ..La caja se procesara por un lote nocturno.");


            $('#message-placeholder').prepend(message);

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
        
        var skp = $('#skp-caja').val();

        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/generar-caja-valorada/${skp}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            console.log(data);

            $('.opened-box').show();
            $('.closed-box').hide();

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
    },
    obtenerCaja: function(){
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
    },
    chequearCaja: function(){

        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/chequear-caja-valorada`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            console.log(data);

            if(!data.status){
                $('.titulo-genera-cajas').text(`Ingreso de SKP para Caja`);
                $('.opened-box').hide();
                $('.closed-box').show();

            }else{
                metodos.obtenerCaja();
            }
            
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

    $('#btn-generar-caja-generica').on("click", function () {
        metodos.generarCaja();
    });

    $('#modal-pistoleo').on('show.bs.modal', function () {
        //metodos.generarCaja();
        metodos.chequearCaja();
    });

    $("#frm-pistoleo").on("submit", function (event) {
        
        event.preventDefault();
        var folio = $('#folio-shot').val();
        var caja = $('#codigo-caja-valorada').val();
        metodos.pistoleo(caja, folio);
        $('#folio-shot').val("");
        
    });

});