window.operateEvents = {
    'change .reparo': function (e, value, row, index) {
        row.reparo = parseInt($(e.target).val());
    }
};

const metodos = {
    avanzarWf: function () {
        debugger;
        let foliosEnvio = [];
        let data = $('#tabla-generica').bootstrapTable('getData');

        foliosEnvio = data.map(function (element) {
            return {
                folioCredito: element.folioCredito,
                reparo: element.reparo != null ? element.reparo : 0
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


        
            $.each(data, function (i, exp) {
                debugger;
                window.open(`/salidas/pdf/detalle-valija-valorada/${exp.codigoSeguimiento}`, "_blank");
            });

        }).fail(function (errMsg) {

            if (errMsg.responseText != "") {
                $.niftyNoty({
                    type: "danger",
                    container: "floating",
                    title: "Error Avance Tareas",
                    message: errMsg.responseText,
                    closeBtn: true,
                    timer: 5000
                });
            }
            else {
                $.niftyNoty({
                    type: "warning",
                    container: "floating",
                    title: "Avance Tareas",
                    message: "Tarea No Finalizada, contacte a Soporte!",
                    closeBtn: true,
                    timer: 5000
                });
            }


        }).always(function () {
            $('#tabla-generica').bootstrapTable('refresh');

        });
    }
}


function formatoReparo(val, row, inc) {
   
    if (row.ejecutadoPor != 'wfboot') {
        row.reparo = 0;
        let salida = `<select class="form-control reparo" id="${inc}">`;
        salida = salida + opcionesReparosAnalisis.map(function (val, index) {
            return `<option value="${index}">${val}</option>`
        });
        salida = salida + `</select>`;
        return salida;
    } else {
        salida = '';
    }
  
}

$(function () {
    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();

    });
});