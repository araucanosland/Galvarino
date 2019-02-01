function linkFolio(val, row, inc) {

    var link = "#";
    var tipoDocumento = $('#tipoDocumento').val();
    switch (tipoDocumento)
    {
        case "nomina-notaria":
            link = `/salidas/pdf/detalle-pack-notaria/${val}`;
        break;
        case "valija-oficina":
            link = `/salidas/pdf/detalle-valijas-oficinas/${val}`;
        break;
        case "valija-valorada":
            link = `/salidas/pdf/detalle-valija-valorada/${val}`;
        break;
        case "caja-valorada":
            link = `/salidas/pdf/detalle-caja-valorada/${val}`;
        break;
    }
    return `<a class="btn btn-link" href="${link}" target="_blank">${val}</button>`;
}


function nroExpedientes(val, row, inc)
{
    return row.expedientes.length;
}