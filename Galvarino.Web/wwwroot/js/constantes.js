const enumTipoDocumentos = new Array('Pagare', 'Fotocopia Cedula Identidad', 'Seguro Desgravamen', 'Seguro de Cesantía', 'Hoja de Prolongacion', 'Acuerdo de Pago')
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
const opcionesReparosAnalisis = [
    'Sin Reparos',
    'Sin Firma ni Huella de Afiliado',
    'Sin Huella Afiliado',
    'Sin Firma Afiliado',
    'Ilegible'
]

const opcionesReparosNotaria = [
    "Sin Reparos",
    'Firma no coincide con cedula ',
    'Sin Timbre de Notario',
    'Sin Firma Ni Timbre de Notario',
    'Sin Firma Notario'
]