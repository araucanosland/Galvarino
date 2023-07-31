window._procesar = [];

const metodos = {
    avanzarWf: function () {
        debugger;
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

        debugger;
        $.each(data, function (index, element) {
            _procesar.push({
                Folio: element.folio,
                Rut: element.rutCliente,
                Nombre: element.nombreCliente,
                Motivo: element.motivo,
                Tipo: element.tipo,
                Fecha: element.fecha
               /* Reparo: element.reparo != null ? element.reparo : 0*/
            });
        });

        debugger;

        $.ajax({
            type: "POST",
            url: `/api/pensionado/wf/v1/despacho-sucursal-oficiana-partes`,
            data: JSON.stringify(_procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            debugger;
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Oficina de Partes",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se cierra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    window.open(`/salidas/pensionado/pdf/detalle-valija-valorada/${data.codigoSeguimiento}`, "_blank");
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

    $('#filter-dp-component .input-group.date').datepicker({ 
        format: "dd-mm-yyyy",
        todayBtn: "linked",
        autoclose: true,
        todayHighlight: true,
        
    }).on('changeDate', function(event){
        console.log({ event })
        if ($('#fecha_filtro').val() !== ''){
            var etapa = $('#la_etapa').val()
            var fecha = $('#fecha_filtro').val()
            $('#tabla-generica').bootstrapTable('refresh', {
                url: `/api/pensionado/wf/v1/mis-solicitudes/${etapa}/${fecha}`
            })
        }
    });

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

    $("#btn-exportar-excel").on("click", function () {
        debugger;
        var value = $("#tabla-generica tr").find('td:first').html();        
        if (value == "No matching records found") {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Generar Excel",
                message: "No puede generar excel, no existen datos a cargar!",
                closeBtn: true,
                timer: 5000
            });
        }
        else {
            var etapa_actual = $("#la_etapa").val();
            location.href = `${BASE_URL}/api/pensionado/reportes/Despacho-Of-Partes/${etapa_actual}`;
        }

    });

});