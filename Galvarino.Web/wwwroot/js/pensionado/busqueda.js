$(function () {


    $("#btn_busqueda_pensionado").on("click", function () {
        debugger;
        var folio = $('#search-input-busqueda').val();
        if (folio == '') {
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error Consulta",
                message: "Para realizar busqueda, se debe ingresar un numero de folio",
                closeBtn: true,
                timer: 5000
            });
            return;
        }
        location.href = `/pensionado/resultado-busqueda/${folio}`;
    });

});