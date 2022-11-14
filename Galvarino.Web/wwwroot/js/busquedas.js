$(function () {


    $("#btn_busqueda_expediente").on("click", function () {
        var folioCredito = $('#search-input-busqueda').val();
        if (folioCredito == '')
            return;
        
        location.href = `/busqueda/resultado-busqueda/${folioCredito}`;
    });

});