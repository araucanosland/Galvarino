const metodos = {
    avanzarWf: function () {

        let codigoSeguimiento = $('#codigoSeguimiento').val();
    
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/recepcion-oficina-partes/${codigoSeguimiento}`
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Recepción de Valija",
                message: "Valija: " + codigoSeguimiento + " Ingresada.<br/><small>Estamos actualizando el estado de la valija..</small>",
                closeBtn: true,
                timer: 5000
            });

        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Ocurrió un problema al procesar Valija",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000
            });

        }).always(function () {
            $('#codigoSeguimiento').val("");
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}

function nroExpedientes(val, row, inc) {
    return row.expedientes.length;
}

$(function(){
    $('#form-pistoleo').on('submit', function(event){


        let data = $('#tabla-generica').bootstrapTable('getData');

        if(data.length > 0){
            metodos.avanzarWf();
        }else{
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Entrada de Valijas",
                message: "Nada que procesar",
                closeBtn: true,
                timer: 5000
            });
            $('#codigoSeguimiento').val("");
        }

        
        event.preventDefault();
    });
})