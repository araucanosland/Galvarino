window.operateEvents = {
    'change .reparo': function (e, value, row, index) {
        row.reparo = parseInt($(e.target).val());
    }
};

const metodos = {
    avanzarWf: function () {
        let foliosEnvio = [];
        let data = $('#tabla-generica').bootstrapTable('getData');

        foliosEnvio = data.map(function(element){
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


function formatoReparo(val, row, inc) {
    return `
    <select class="form-control reparo" id="${inc}">
        <option value="0">Sin Reparos</option>
        <option value="1">Sin Firma de Notario</option>
        <option value="2">Sin Timbre de Notario</option>
        <option value="3">Sin Firma ni Timbre</option>
        <option value="4">Ilegible</option>
    </select>`;
    /*TODO: Aqui hay datos en duro */
}

function bloqueoBoton(selector, option='start')
{
    if(option == 'start')
    {
        $(selector).data("text", $(selector).text());
        $(selector).text("Cargando...");
        $(selector).prop({
            disabled: true
        });
    }

    if(option == 'end')
    {
        $(selector).text($(selector).data('text'));
        $(selector).prop({
            disabled: false
        });
    }
}

$(function () {

    
    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
        
    });

    

});