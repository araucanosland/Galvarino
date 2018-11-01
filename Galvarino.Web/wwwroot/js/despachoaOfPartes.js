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
            url: `/api/wf/v1/despacho-sucursal-oficiana-partes`,
            data: JSON.stringify(_procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Oficina de PArtes",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    window.open(`/salidas/pdf/detalle-valija-valorada/${data.codigoSeguimiento}/OF_PARTES`, "_blank");
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
            console.log("always");
            _procesar = [];
            $('#tabla-generica').bootstrapTable('refresh');
        });

    }
}



$(function () {


    $("#btn-generar-generico").on("click", function () {
        console.log('Generando')
        metodos.avanzarWf();
    });

});