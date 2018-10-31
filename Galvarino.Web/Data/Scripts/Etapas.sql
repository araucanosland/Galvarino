/*BEGIN;
INSERT INTO `Etapas` (`Id`, `ProcesoId`, `TipoEtapa`, `Nombre`, `NombreInterno`, `TipoUsuarioAsignado`, `ValorUsuarioAsignado`, `TipoDuracion`, `ValorDuracion`, `TipoDuracionRetardo`, `ValorDuracionRetardo`, `Secuencia`) 
VALUES  ('1', '1', 'Inicio', 'Inicio', 'INICIO', 'Boot', 'wfboot', 'Ninguna', null, 'Ninguna', null, '100'), 
        ('2', '1', 'Actividad', 'Carga Inicial', 'CARGA_INICIAL', 'Boot', 'wfboot', 'Ninguna', null, 'Ninguna', null, '200'), 
        ('3', '1', 'Actividad', 'Despacho a Oficina Notaría', 'DESPACHO_OFICINA_NOTARIA', 'Rol', 'Sucursal', 'Ninguna', null, 'Ninguna', null, '300'), 
        ('4', '1', 'Actividad', 'Recepción Set Legal', 'RECEPCION_SET_LEGAL', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '400'), 
        ('5', '1', 'Actividad', 'Envío a Notaría', 'ENVIO_NOTARIA', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '400'), 
        ('6', '1', 'Actividad', 'Recepción de Notaría', 'RECEPCION_NOTARIA', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '500'), 
        ('7', '1', 'Actividad', 'Revisión de Documentos', 'REVISION_DOCUMENTOS', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '500'), 
        ('8', '1', 'Actividad', 'Despacho a Notaría por Reparo', 'DESPACHO_REPARO_NOTARIA', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '600'), 
        ('9', '1', 'Actividad', 'Despacho de Sucursal a Oficina de Partes', 'DESPACHO_OFICINA_PARTES', 'Auto', 'USUARIO_OF_PROCESA', 'Ninguna', null, 'Ninguna', null, '700'), 
        ('10', '1', 'Actividad', 'Recepción Valija Oficina de Partes', 'RECEPCION_VALIJA_OFICINA_PARTES', 'Rol', 'Oficina de Partes', 'Ninguna', null, 'Ninguna', null, '800'), 
        ('11', '1', 'Actividad', 'Recepción Valija Mesa Control', 'RECEPCION_VALIJA_MESA_CONTROL', 'Rol', 'Mesa de Control', 'Ninguna', null, 'Ninguna', null, '900'),
        ('12', '1', 'Actividad', 'Analisis Mesa Control', 'ANALISIS_MESA_CONTROL', 'Rol', 'Mesa de Control', 'Ninguna', null, 'Ninguna', null, '900'),
        ('13', '1', 'Actividad', 'Despacho a Of. Partes Para Devolución a Sucursal', 'DESPACHO_OF_PARTES_DEVOLUCION', 'Rol', 'Mesa de Control', 'Ninguna', null, 'Ninguna', null, '900'),
        ('14', '1', 'Actividad', 'Despacho a Of. Para Corrección', 'DESPACHO_OFICINA_CORRECCION', 'Rol', 'Oficina de Partes', 'Ninguna', null, 'Ninguna', null, '900'),
        ('15', '1', 'Actividad', 'Solución Reparos Sucursal', 'SOLUCION_REPAROS_SUCURSAL', 'Rol', 'USUARIO_OF_PROBLEMAS', 'Ninguna', null, 'Ninguna', null, '900'),
        ('16', '1', 'Actividad', 'Despacho a Custodia', 'DESPACHO_A_CUSTODIA', 'Rol', 'Mesa de Control', 'Ninguna', null, 'Ninguna', null, '900'),
        ('17', '1', 'Actividad', 'Recepción y Custodia', 'RECEPCION_Y_CUSTODIA', 'Rol', 'Iron Mountain', 'Ninguna', null, 'Ninguna', null, '900'),
        ('18', '1', 'Fin', 'Fin', 'FIN', 'Boot', 'wfboot', 'Ninguna', null, 'Ninguna', null, '900');
COMMIT;
*/