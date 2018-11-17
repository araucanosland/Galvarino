window._procesar = [];


window.operateEvents = {
    'change .reparo': function (e, value, row, index) {
        row.reparo = parseInt($(e.target).val());
    }
};


const metodos = {
    avanzarWf: function () {

        let data = $('#tabla-generica').bootstrapTable('getData');

        $.each(data, function (index, element) {
            _procesar.push({
                folioCredito: element.expediente.credito.folioCredito,
                reparo: element.reparo
            });
        });

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/revision-documentos-rm`,
            data: JSON.stringify(_procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Revisión Documentos",
                message: "Tarea Finalizada con Éxito.<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al tus solicitudes</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    location.href = "/wf/v1/mis-solicitudes";
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

function formatoReparo(val, row, inc) {
    return `
    <select class="form-control reparo" id="${inc}">
        <option value="0">Sin Reparos</option>
        <option value="1">Sin Firma de Notario</option>
        <option value="2">Sin Timbre de Notario</option>
        <option value="3">Sin Firma ni Timbre</option>
    </select>`;
}


function formatoTackingPack(val, row, inc) {
    return `<span class="label label-table label-info">${val}</span>`;
}

$(function () {


    /*$('#tabla-generica').on('all.bs.table', function (name, args) {
        console.log(name, args)
    });*/

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});