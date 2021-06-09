window._ingresadosAux = [];

const metodos = {
    
    render: function (_ingresados = []) {

        debugger;
        $("#total-expedientes").html(`Expedientes pistoleados: <strong>${_ingresados.length}</strong>`);
        
        $('.contenedor-folios').html("");
        $.each(_ingresados, function (i, exp) {
            debugger;
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
        debugger;
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/${codigoCaja}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

           
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
        debugger;
        //TODO: se debe enviar señal al servidor con documento agregandolo a una caja
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/caja-valorada/${codigoCaja}/agregar-documento/${folioDocumento}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            debugger;
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
        debugger;
        var skp = $('#skp-caja').val();
        if (skp == "") {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error al generar Caja",
                message: "Debe Ingresar SKP",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }

        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/generar-caja-valorada/${skp}`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            
            console.log(data);
            debugger;
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
    obtenerCaja: function () {
        debugger;
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/despacho-a-custodia/generar-caja-valorada`,
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            debugger;
            console.log(data);
            $('.titulo-genera-cajas').text(`Generando Caja Folio: ${data.caja.codigoSeguimiento}`);
            $('#codigo-caja-valorada').val(data.caja.codigoSeguimiento);
            metodos.render(data.documentos);
            debugger;
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
        debugger;
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