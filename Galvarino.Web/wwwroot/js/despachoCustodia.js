
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

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Custodia",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    window.open(`/salidas/pdf/detalle-caja-valorada/${data.codigoSeguimiento}`, "_blank");
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
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}



$(function () {

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});