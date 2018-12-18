let variables = {
    incrementor: 0
}
const metodos = {
    guaradar: function () {
        let cantidadAdmitida = parseInt($('#cantidad-expedientes').val());
        let foliosEnvio = [];
        let data = $('#tabla-generica').bootstrapTable('getData');


        

        if(data.length < cantidadAdmitida)
        {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Validación de Negocio!",
                message: "Debes tener almenos la cantidad de expedientes que pretendes guardar",
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
                timer: 5000,
                onHidden: function()
                {
                    window.open(`/salidas/pdf/detalle-caja-comercial/${data.codigoSeguimiento}`, "_blank");
                }
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
        if ($('#cantidad-expedientes').val().length == 0)
        {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error de Usuario",
                message: "Debes Ingresar una cantidad de Expedientes a Guardar",
                closeBtn: true,
                timer: 5000
            });

            return false;
        }
        metodos.guaradar();

    });



});


function formatoIncrementor(val, row, inc){
    let mostrar = inc+1;
    return `<span>${mostrar}</span>`;
}

function formatoLinked(val, row, inc){
    return `<a class="btn btn-link" target="_blank" href="/salidas/pdf/detalle-caja-comercial/${row.codigoSeguimiento}">${row.codigoSeguimiento}</a>`;
}