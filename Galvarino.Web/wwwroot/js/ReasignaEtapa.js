
function Limpiar() {
    location.re();
    return;
}

function ValidaInput() {
    var inputBusqueda = $('#inputBusqueda').val();
    if (inputBusqueda === "") {
        $('#btn_guardar').attr('disabled', true);
        $('#nuevaEtapa').attr('disabled', true);
    }
    else {
        $('#btn_guardar').attr('disabled', false);
        $('#nuevaEtapa').attr('disabled', false);
    }
}

$(function () {
    $('#inputBusqueda').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    //if (sessionStorage.getItem('folio') != null)
    //    $("#inputBusqueda").val(sessionStorage.getItem('folio'));
    
    $("#form-busqueda").on("submit", function (event) {        
        var inputBusqueda = $('#inputBusqueda').val();
        $.ajax({
            type: "GET",
            url: `/api/wf/v1/reasignaciones/etapas`,
            data: {
                q: inputBusqueda
            }
        }).done(function (data) {
            if (typeof data !== 'undefined') {
                //$('#folio-credito').val(data.Credito.folioCredito)
                
                $('#etapaactual').val(data.etapa.nombre);
                $('#nuevaEtapa').attr('disabled', false);
                $('#hdasignado').val(data.tareas.asignadoA);
            } else {
                $.niftyNoty({
                    type: "warning",
                    container: "floating",
                    title: "Advertencia",
                    message: "No encontramos lo que buscas...<br/><small>Favor Verificar Folio!!!</small>",
                    closeBtn: true,
                    timer: 5000
                });

                $('#nuevaEtapa').attr('disabled', true);
            }
        }).fail(function (err) {
            $('#datos-credito').html("<h1>No encontramos lo que buscas!</h1>");
            console.log({ err });
        });
        return false;
    });

    
  

    $("#btn_guardar").on("click", function () {
 
        event.preventDefault();
    

        if ($('#inputBusqueda').val().length === 0) {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Advertencia",
                message: "Debe ingresar folio!",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }
        
        if ($('#nuevaEtapa').val() === "0") {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Advertencia",
                message: "No fue seleccionada la Etapa!",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }
       /* if ($('#hdasignado').val() === "Mesa Control") {
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Advertencia",
                message: "Folio no puede estar asignado a Mesa de Control!",
                closeBtn: true,
                timer: 5000
            });
            return false;
        }*/
        let datos = {
            folio: $('#inputBusqueda').val(),
            idetapa: $('#nuevaEtapa').val()
        };
        $.ajax({

            type: "POST",
            url: `/api/wf/v1/reasignaciones/actualizaretapas`,
            data: JSON.stringify(datos),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
           
            if (data.estado === "OK") {
                $.niftyNoty({
                    type: "success",
                    container: "floating",
                    title: "Modificando Etapa",
                    message: data.mensaje,
                    closeBtn: true,
                    timer: 5000
                });
            }
            else if (data.estado === "Bad") {
                $.niftyNoty({
                    type: "warning",
                    container: "floating",
                    title: "Modificando Etapa",
                    message: data.mensaje,
                    closeBtn: true,
                    timer: 5000
                });
            }
            
            
        }).fail(function (errMsg) { 
  
            $.niftyNoty({
                type: "warning",
                container: "floating",
                title: "Modificando Etapa",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000
            });                

        });  
        
    });
});