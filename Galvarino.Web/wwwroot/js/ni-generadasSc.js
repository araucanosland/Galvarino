function linkFolio(val, row, inc) {
    debugger;
    var link = "#";
    var tipoDocumento = $('#tipoDocumento').val();
    switch (tipoDocumento) {
        case "valija-oficina-documento-of-Pago":
            link = `/wf/v1/detalle-valija-valorada-Doc-Sucursal-sc/${val}`;
            break;
         case "valija-valorada":
            link = `/wf/v1/detalle-valija-valorada-sc/${val}`;
            break;
 
    }
    return `<a class="btn btn-link" href="${link}" target="_blank">${val}</button>`;
}


function nroExpedientes(val, row, inc) {
    debugger;
    return row.expedientes.length;
}