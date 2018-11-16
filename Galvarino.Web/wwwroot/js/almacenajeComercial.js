const metodos = {
    guaradar: function () {
        let foliosEnvio = [];
        let data = $('#tabla-generica').bootstrapTable('getData');

        foliosEnvio = data.map(function (element) {
            return {
                folioCredito: element.expediente.credito.folioCredito,
                reparo: element.reparo
            }
        });

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/analisis-mesa-control`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Análisis Mesa de Control",
                message: "Documentos Procesados.<br/><small>Estamos actualizando el estado de las tareas.</small>",
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
        metodos.guaradar();

    });



});