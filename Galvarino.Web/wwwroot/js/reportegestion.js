$.fn.datepicker.dates['es'] = {
    days: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"],
    daysShort: ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"],
    daysMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sá"],
    months: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"],
    monthsShort: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
    today: "Hoy",
    clear: "Borrar",
    format: "dd/mm/yyyy",
    titleFormat: "MM yyyy", /* Leverages same syntax as 'format' */
    weekStart: 1
};


function formatoPendienteReporte(val, row, inc) {
    debugger;
    if (row.estado == 'Pendiente') {
        
        return `<strong>${row.estado}</strong> `
    }
    else {
        return `${row.estado} `  }  
       
   
  
}



$(function () {

    var hoy = new Date();
    var tomorrow = new Date(hoy);
    tomorrow.setDate(tomorrow.getDate() + 1)
    var d = hoy.getDate().toString() + "-" + (hoy.getMonth() + 1).toString() + "-" + hoy.getFullYear().toString();
    var m = tomorrow.getDate().toString() + "-" + (tomorrow.getMonth() + 1).toString() + "-" + tomorrow.getFullYear().toString();
    $("#dp_dia_desde").val(d)
    $("#dp_dia_hasta").val(d)
    $("#dp_dia_ejecucion").val(m)


    $('#dp-component .input-group.date').datepicker({
       
        autoclose: true,
        format: 'dd-mm-yyyy',
        language: "es",
        todayHighlight: true,
        //startDate: '-7d'
    }
    ).on('changeDate', function (e) {
   
    });


    $("#btn_crear_reporte").on("click", function () {

        var fechadesde = $("#dp_dia_desde").val();
        var fechahasta = $("#dp_dia_hasta").val();
        var fechaejecucion = $("#dp_dia_ejecucion").val();

        fechadesde = fechadesde.replace("-", "/").replace("-", "/");
        var newfechadesde = fechadesde.replace(/(\d+[/])(\d+[/])/, '$2$1');
        var fdesde = new Date(newfechadesde);

        fechahasta = fechahasta.replace("-", "/").replace("-", "/");
        var newfechahasta = fechahasta.replace(/(\d+[/])(\d+[/])/, '$2$1');
        var fhasta = new Date(newfechahasta);
        
        if (fdesde > fhasta) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Suceso Erroneo",
                message: "La fecha inicial debe ser menor a Fecha final!",
                closeBtn: false,
                timer: 5000
            });
            return false;
        }

        fechaejecucion = fechaejecucion.replace("-", "/").replace("-", "/");
        var newfechaejecucion = fechaejecucion.replace(/(\d+[/])(\d+[/])/, '$2$1');
        var fejecucion = new Date(newfechaejecucion);
        var ahora = new Date();
       
        if (ahora >= fejecucion) {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Suceso Erroneo",
                message: "La fecha ejecucion debe ser mayor a hoy!",
                closeBtn: false,
                timer: 5000
            });
            return false;
        }

        let _data = {
            FechaInicial : $("#dp_dia_desde").val(),
            FechaFinal: $("#dp_dia_hasta").val(),
            FechaEjecucion: $("#dp_dia_ejecucion").val()
        }

        $.ajax({
            type: "POST",
            url: `/api/reportes/Crear-Fecha-reporte-programado`,
            data: JSON.stringify(_data),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {

            if (data.estado === "OK") {
                $.niftyNoty({
                    type: "success",
                    container: "floating",
                    title: "Reporte Programado",
                    message: data.mensaje,
                    closeBtn: true,
                    timer: 5000
                });
                $('#tabla-generica').bootstrapTable('refresh');
            }
        }).fail(function (errMsg) {

            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Reporte Programado",
                message: "No se puede generar otro reporte, ya se encuentra un reporte pendiente para ejecutar día " + fechaejecucion,
                closeBtn: true,
                timer: 5000
            });
        });
    });

});