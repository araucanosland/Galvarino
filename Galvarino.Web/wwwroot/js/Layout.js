
$(function () {
  
    let etapa = $("#etapa-solucion-reparos-sc").val();
    $.ajax({
        type: "GET",
        url: `/api/app/v1/obtener-canitdad/Etapa-Sc/${etapa}`
    }).done(function (data) {
       // <span id="sd" class="badge badge-danger pull-right">0</span>
        //expediente.obtenido = data;
      
        if (data == 0) {
            $("#alerta").html("");
        }
        else {
                            
            $("#alerta").html(`<span class="badge badge-danger pull-right">${data}</span>`);
        }
        //let index = _ingresados.findIndex(function (expedientey) {
        //    return expedientey.folioCredito == expediente.folioCredito
        //});

        //if (index > -1 && typeof _ingresados[index] != 'undefined' && expediente.folioCredito === _ingresados[index].folioCredito) {
        //    if (_ingresados[index].documentos.indexOf(expediente.codigoTipoDocumento) < 0) {
        //        _ingresados[index].documentos.push(expediente.codigoTipoDocumento)
        //    }
        //} else {
        //    if (expediente.obtenido.id > 0) {
        //        _ingresados.push(expediente);
        //    }
        //}

        //$("#folio-shot").val("");
        //metodos.render();

    })   


});