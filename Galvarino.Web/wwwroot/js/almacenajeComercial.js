let variables = {
    incrementor: 0
}
const metodos = {
    guaradar: function () {
        let cantidadAdmitida = 70;
        let foliosEnvio = [];
        let data = $('#tabla-generica').bootstrapTable('getData');


        if(data.length < cantidadAdmitida)
        {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Validación de Negocio!",
                message: "Deben haber al menos 80 expedientes en la lista para poder cerrar una caja.",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }

        
        foliosEnvio = data.map(function (element, indx) {

            
            if(indx < cantidadAdmitida)
            {
                return {
                    folioCredito: element.expediente.credito.folioCredito,
                }
            }
        });

        foliosEnvio = foliosEnvio.filter(function (el) {
            return el != null;
          });

        console.log({foliosEnvio, data})

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/almacenaje-set-comercial`,
            data: JSON.stringify(foliosEnvio),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Control Documentos Comerciales",
                message: "Documentos Procesados.<br/><small>Estamos actualizando el listado de Documentos.</small>",
                closeBtn: true,
                timer: 5000
            });

        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Error Al procesar Documentos",
                message: "Tarea No Finalizada, contacte a Soporte!",
                closeBtn: true,
                timer: 5000
            });

        }).always(function () {
            variables.incrementor = 0;
            $('#tabla-generica').bootstrapTable('refresh');
        });
    }
}


$(function () {


    $("#btn-generar-generico").on("click", function () {
        metodos.guaradar();

    });



});


function formatoIncrementor(val, row, inc){
    let mostrar = inc+1;
    return `<span>${mostrar}</span>`;
}