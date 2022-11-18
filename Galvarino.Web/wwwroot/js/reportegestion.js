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




$(function () {
  
    var hoy = new Date();
    var d = hoy.getDate().toString() + "-" + (hoy.getMonth() + 1).toString() + "-" + hoy.getFullYear().toString();
    $("#dp_dia_desde").val(d)
    $("#dp_dia_hasta").val(d)



    $('#dp-component .input-group.date').datepicker({
       
        autoclose: true,
        format: 'dd-mm-yyyy',
        language: "es",
        todayHighlight: true,
        //startDate: '-7d'
    }
    ).on('changeDate', function (e) {
   
    });

    //$('#frm-generico').on('submit', function (e) {
    //    e.preventDefault();
    //    debugger;
    //    $(".submit-generico").html("Cargando...");
    //    var fechadesde = $("#dp_dia_desde").val();
    //    var fechahasta = $("#dp_dia_hasta").val();
    //    if (fechadesde > fechahasta)
    //    {
    //        $.niftyNoty({
    //            type: "error",
    //            container: "floating",
    //            title: "Suceso Erroneo",
    //            message: "La fecha inicial debe ser menor a Fecha final!",
    //            closeBtn: false,
    //            timer: 5000
    //        });
    //    }
    //});



    $("#btn_crear_reporte").on("click", function () {
        debugger;
        var fechadesde = $("#dp_dia_desde").val();
        var fechahasta = $("#dp_dia_hasta").val();
        if (fechadesde > fechahasta) {
            $.niftyNoty({
                type: "error",
                container: "floating",
                title: "Suceso Erroneo",
                message: "La fecha inicial debe ser menor a Fecha final!",
                closeBtn: false,
                timer: 5000
            });
            return false;
        }

        let _data = {
             FechaInicial : $("#dp_dia_desde").val(),
             FechaFinal : $("#dp_dia_hasta").val()
        }
       
        //var inicial = $("#dp_dia_desde").val();
        //var final = $("#dp_dia_hasta").val();

        $.ajax({
            type: "POST",
            url: `/reportes/workflow/reporte-programado`,
            data: JSON.stringify(_data),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            debugger;
        }).fail(function (errMsg) {

        }).always(function () {

        });
    });

});