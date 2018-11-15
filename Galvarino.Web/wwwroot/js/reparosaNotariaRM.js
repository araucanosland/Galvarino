window._procesar = [];

const metodos = {
    avanzarWf: function () {

        let data = $('#tabla-generica').bootstrapTable('getData');


        if (data.length == 0) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Avance Tareas",
                message: "Nada para enviar!",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }


        $.each(data, function (index, element) {
            _procesar.push({
                FolioCredito: element.expediente.credito.folioCredito,
                Reparo: element.reparo
            });
        });



        $.ajax({
            type: "POST",
            url: `/api/wf/v1/despacho-reparo-notaria-rm`,
            data: JSON.stringify(_procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho Reparos a Notaría",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    location.href = "/wf/v1/mis-solicitudes";
                    window.open(`/salidas/pdf/detalle-pack-notaria/${data.codigoSeguimiento}`, "_blank");
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
            _procesar = [];
            $('#tabla-generica').bootstrapTable('refresh');
        });

    }
}

function formatoReparo(val, row, inc) {

    let opciones = [
        'Sin Reparos',
        'Sin Firma de Notario',
        'Sin Timbre de Notario',
        'Sin Firma ni Timbre',
        'Ilegible'
    ]

    return opciones[val];
}


$(function () {
    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });
});