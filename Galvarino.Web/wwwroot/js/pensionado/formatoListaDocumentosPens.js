function formatoListaDocumentosPens(val, row, inc) {
    let strOut = row.documento.map(function (val, idx) {
        return `<strong>${val.codificacion}</strong> - ${val.nombreDocumento}`
    });
    return strOut.join('<br />');
}
