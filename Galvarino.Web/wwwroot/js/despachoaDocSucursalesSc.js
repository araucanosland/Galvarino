﻿window._procesar = [];

const metodos = {
    avanzarWf: function () {

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


        $.each(data, function (index, element) {
            _procesar.push({
                FolioCredito: element.folioCredito
            });
        });



        $.ajax({
            type: "POST",
            url: `/api/wfSc/v1/despacho-Documentos-opartes-Sucursal-sc`,
            data: JSON.stringify(_procesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            debugger;
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Despacho a Oficina de Partes",
                message: "Estamos Generando la Nómina...<br/><small>Esta Tarea se ceirra en 5 Seg. y te redirige al Pdf de La Nómina de envío</small>",
                closeBtn: true,
                timer: 5000,
                onHidden: function () {
                    debugger;
                    window.open(`/wf/v1/detalle-valija-valorada-Doc-Sucursal-sc/${data.codigoSeguimiento}`, "_blank");
                    window.location.reload(true);
                }
                
               
                    
            });


            }
            
        ).fail(function (errMsg) {
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

    $('#btn-generar-generico').attr('disabled', true);

    debugger;
 


    $("#sucursales").on("change", function () {
        debugger;
        var etapa = $("#etapa").val();
        var sucursales = $("#sucursales").val();
        if (sucursales !='AA00' ) {
            $('#btn-generar-generico').attr('disabled', false);
        }
        else
        {
            $('#btn-generar-generico').attr('disabled', true);
        }
        $("#tabla-generica").bootstrapTable('refresh', {
            url: `/api/wfSc/v1/nomina-setcomplementario/${etapa}?sucursal=${sucursales}`
        });
    })


    $('#filter-dp-component .input-group.date').datepicker({
        format: "dd-mm-yyyy",
        todayBtn: "linked",
        autoclose: true,
        todayHighlight: true,

    }).on('changeDate', function (event) {
        console.log({ event })
        if ($('#fecha_filtro').val() !== '') {
            var etapa = $('#la_etapa').val()
            var fecha = $('#fecha_filtro').val()
            $('#tabla-generica').bootstrapTable('refresh', {
                url: `/api/wfSc/v1/nomina-setcomplementario/${etapa}/${fecha}`
            })
        }
    });

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});