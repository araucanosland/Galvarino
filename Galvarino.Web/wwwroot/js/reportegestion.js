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



    $('#dp-component .input-group.date').datepicker({
       
        autoclose: true,
        format: 'dd-mm-yyyy',
        language: "es",
        todayHighlight: true,
        //startDate: '-7d'
    }
    ).on('changeDate', function (e) {
   
    });

});