const enumTipoDocumentos = new Array('Pagare', 'Fotocopia Cedula Identidad', 'Solicitud Credito', 'Liquidacion', 'Hoja de Prolongacion', 'Acuerdo de Pago')
const enumTipoCreditos = new Array('Normal', 'Reprogramación', 'Acuerdo Pago')
const formatoFolios = {
    codigo: {
        inicio: 0,
        fin: 2
    },
    folioCredito: {
        inicio: 2,
        fin: 14
    },
    rutAfiliado: {
        inicio: 15,
        fin: 23
    }
}
const opcionesReparosNotaria = [
    'Sin Reparos',
    'Sin Firma de Notario',
    'Sin Timbre de Notario',
    'Sin Firma ni Timbre',
    'Ilegible'
]