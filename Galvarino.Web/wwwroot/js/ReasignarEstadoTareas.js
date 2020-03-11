function Limpiar() {
    location.reload();
    return;
}


$(function () {



    $("#btn_modificar").on("click", function () {
        debugger;
        event.preventDefault();
        var model = {
            folioCredito: $('#inputBusqueda').val(),
            nuevaEtapa: $("#cmb_etapas").val()
        }

        // Carga información de  oficinas
        $.ajax({
            type: "POST",
            url: `/api/wf/v1/reasignaciones/tareas`,
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Reasignar Etapa",
                message: "Etapa Modificada...<br/><small>Correctamente!!!</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {

                },


            })

        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Avance Tareas",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000

            });
        })

    });


    $("#btn_modificar--").click(function () {
        debugger;
        event.preventDefault();
        var model = {
            folioCredito: $('#folio-credito').val(),
            nuevaOficina: $("#nuevaOficina").val(),
            tipo: 'Legal'
        }

        console.log({
            model
        })




        // Carga información de  oficinas
        $.ajax({
            type: "POST",
            url: `/api/wf/v1/reasignaciones/oficinaevaluadora`,
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Reasignar Etapa",
                message: "Etapa Modificada...<br/><small>Correctamente!!!</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {

                },


            })

        }).fail(function (errMsg) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Avance Tareas",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000

            });
        })
    });


    $("#form-busqueda").on("submit", function (event) {
        debugger;
        var inputBusqueda = $('#inputBusqueda').val();

        $('#folio-credito').text("")
        $('#rut-afiliado').text("")
        $('#etapa-actual').text("")
        $('#asignado-a').text("")
        $('#cmb_etapas').attr('disabled', true);
        $.ajax({
            type: "GET",
            url: `/api/mantenedores/obtener-etapa/tareas/${inputBusqueda}`
          
        }).done(function (data) {
            debugger;
                $('#folio-credito').text(data.credito.folioCredito)
                $('#rut-afiliado').text(data.credito.rutCliente)
                $('#etapa-actual').text(data.tareasPagres[0].etapa.nombre)
                $('#asignado-a').text(data.tareasPagres[0].asignadoA)
                $('#cmb_etapas').attr('disabled', false);
                $("#cmb_etapas").empty();
                $("#cmb_etapas").append('<option value="">Selecciona</option>');
                $.each(data.etapaTareas, function (key, registro) {
                    $("#cmb_etapas").append('<option value=' + registro.id + '>' + registro.nombre + '</option>');

                })
                                           
        }).fail(function (err) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Error de búsqueda",
                message: "Folio no encontrado!!!!",
                closeBtn: true,
                timer: 5000

            });
            console.log({ err })
          
        })

        return false;
    });




});

$(function () {
    debugger;
    $('#datos-credito').html("")
    $('#cmb_etapas').attr('disabled', true);
    $('#inputBusqueda').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

   



});
