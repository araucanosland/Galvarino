window._procesar = [];
window._reparosProcesar = [];

window.operateEvents = {
    'change .reparo': function (e, value, row, index) {
        row.reparo = parseInt($(e.target).val());
    }
};


const metodos = {
    avanzarWf: function () {



       /* var id = $(this).attr('value');
        $(".table").find("tr").find("select").each(function () {

            var valorSelect = $(this).val();



        });*/
        var reparo, folioCredito;
        let i=0;
        $("#tabla-generica tbody tr").each(function (index) {
           
           
            $(this).children("td").each(function (index2) {
               
                switch (index2) {
                  
                    case 0:
                        $(".table").find("tr").find("select").each(function (fila) {
                           
                            if (fila==i)
                                reparo = $(this).val();
                            return;
                        });
                                                
                        break;

                    case 1:
                        folioCredito = $(this).text();
                        break;
             }
            });
          
            _reparosProcesar.push({
                folioCredito: folioCredito,
                reparo: reparo
            });
           
            i++;
        });
        debugger;
        console.log(_reparosProcesar)

        /*let data = $('#tabla-generica').bootstrapTable('getData');

        $.each(data, function (index, element) {
            debugger;
            _procesar.push({
                folioCredito: element.folioCredito,
                reparo: element.reparo != null ? element.reparo : 0
            });
        });*/

        $.ajax({
            type: "POST",
            url: `/api/wf/v1/revision-documentos-rm`,
            data: JSON.stringify(_reparosProcesar),
            contentType: "application/json; charset=utf-8"
        }).done(function (data) {
            $.niftyNoty({
                type: "success",
                container: "floating",
                title: "Revisión Documentos",
                message: "Tarea Finalizada con Éxito.<br/><small>Esta Tarea se cierra en 5 Seg. y te redirige al tus solicitudes</small>",
                closeBtn: true,
                timer: 5000,

            });
            debugger;
            $.each(data, function (i, exp) {
                debugger;
                window.open(`/salidas/pdf/detalle-valija-valorada/${exp.codigoSeguimiento}`, "_blank");
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
            $('#tabla-generica').bootstrapTable('refresh');

        });

    }
}

function formatoReparo(val, row, inc) {

    let salida = `<select class="form-control reparo" id="ddlreparos">`;
    salida = salida + opcionesReparosNotaria.map(function (val, index) {
        return `<option value="${index}">${val}</option>`
    });
    salida = salida + `</select>`;
    return salida;
}


function formatoTackingPack(val, row, inc) {
    return `<span class="label label-table label-info">${val}</span>`;
}

$(function () {

    $("#btn-generar-generico").on("click", function () {
        metodos.avanzarWf();
    });

});