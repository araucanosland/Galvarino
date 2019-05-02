const metodos = {
    avanzarWf: function () {

        let data = $('#tabla-generica').bootstrapTable('getSelections');
        let procesar = data.map(function(element){
            return {
                folioCredito: element.folioCredito
            }
        })
       
        $.ajax({
            type: "POST",
            url: `/api/wf/v1/solucion-reparo-sucursal`,
            data: JSON.stringify(procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Solución de Reparos",
                message: "Estamos procesando los reparos en los expedientes.<br/><small>Esta notificación se ceirra en 5 Seg.</small>",
                closeBtn: true,
                timer: 5000
            });

        }).fail(function (errMsg) {

            console.log({
                errMsg
            })
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
    
    return opcionesReparosNotaria[val];
}


$(function () {

    $("#btn-generar-generico").on("click", function () {   
        metodos.avanzarWf();
    });

});