

const metodos = {
   
    avanzarWf: function (entrada) {
        let processData = $(entrada.destiny).bootstrapTable('getData');

        let foliosEnvio = processData.map(function(item){
            return {
                folioCredito: item.expediente.credito.folioCredito
            };
        });

        var postValues = {
            codOficina: entrada.oficina,
            expedientesGenericos: foliosEnvio
        };

        
        $.ajax({
            type: "POST",
            url: `/api/wf/v1/despacho-reparo-oficina-partes`,
            data: JSON.stringify(postValues),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Generar Valija con Reparos",
                message: "Estamos Generando la Nómina...<br/><small>Esta Valija se Genera en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    window.open(`/salidas/pdf/detalle-valija-valorada/${data.codigoSeguimiento}`, "_blank");
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
            $(entrada.destiny).bootstrapTable('refresh');
        });
    }
}


$(function(){
    $('.avanzarwf').on('click', function(){
        var destiny = $(this).data('destiny');
        var oficina = $(this).data('oficina');
        metodos.avanzarWf({destiny, oficina});
    });
});