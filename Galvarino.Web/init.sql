IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [CargasIniciales] (
    [Id] int NOT NULL IDENTITY,
    [FechaCarga] datetime2 NOT NULL,
    [FechaCorresponde] datetime2 NOT NULL,
    [FolioCredito] nvarchar(max) NOT NULL,
    [RutAfiliado] nvarchar(max) NULL,
    [CodigoOficinaIngreso] nvarchar(max) NULL,
    [CodigoOficinaPago] nvarchar(max) NULL,
    [LineaCredito] nvarchar(max) NULL,
    [RutResponsable] nvarchar(max) NULL,
    [CanalVenta] nvarchar(max) NULL,
    [Estado] nvarchar(max) NULL,
    [FechaVigencia] nvarchar(max) NULL,
    [NombreArchivoCarga] nvarchar(max) NULL,
    CONSTRAINT [PK_CargasIniciales] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [ConfiguracionDocumentos] (
    [Id] int NOT NULL IDENTITY,
    [TipoDocumento] int NOT NULL,
    [TipoExpediente] int NOT NULL,
    [TipoCredito] int NOT NULL,
    CONSTRAINT [PK_ConfiguracionDocumentos] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Creditos] (
    [Id] int NOT NULL IDENTITY,
    [FolioCredito] nvarchar(max) NULL,
    [MontoCredito] bigint NOT NULL,
    [FechaFormaliza] datetime2 NOT NULL,
    [FechaDesembolso] datetime2 NOT NULL,
    [RutCliente] nvarchar(max) NULL,
    [NombreCliente] nvarchar(max) NULL,
    [NumeroTicket] nvarchar(max) NULL,
    [TipoCredito] int NOT NULL,
    CONSTRAINT [PK_Creditos] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Organizaciones] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [TipoOrganizacion] nvarchar(max) NOT NULL,
    [PadreId] int NULL,
    CONSTRAINT [PK_Organizaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Organizaciones_Organizaciones_PadreId] FOREIGN KEY ([PadreId]) REFERENCES [Organizaciones] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Procesos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(150) NOT NULL,
    [NombreInterno] nvarchar(max) NULL,
    [NamespaceGeneraTickets] nvarchar(max) NULL,
    [ClaseGeneraTickets] nvarchar(max) NULL,
    [MetodoGeneraTickets] nvarchar(max) NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Procesos] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Regiones] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Secuencia] int NOT NULL,
    CONSTRAINT [PK_Regiones] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Variables] (
    [Id] int NOT NULL IDENTITY,
    [NumeroTicket] nvarchar(max) NULL,
    [Clave] nvarchar(max) NULL,
    [Valor] nvarchar(max) NULL,
    [Tipo] nvarchar(max) NULL,
    CONSTRAINT [PK_Variables] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Roles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [OrzanizacionId] int NOT NULL,
    [PadreId] nvarchar(450) NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Roles_Organizaciones_OrzanizacionId] FOREIGN KEY ([OrzanizacionId]) REFERENCES [Organizaciones] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Roles_Roles_PadreId] FOREIGN KEY ([PadreId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Etapas] (
    [Id] int NOT NULL IDENTITY,
    [ProcesoId] int NOT NULL,
    [TipoEtapa] nvarchar(max) NOT NULL,
    [Nombre] nvarchar(max) NULL,
    [NombreInterno] nvarchar(max) NULL,
    [TipoUsuarioAsignado] nvarchar(max) NOT NULL,
    [ValorUsuarioAsignado] nvarchar(max) NOT NULL,
    [TipoDuracion] nvarchar(max) NOT NULL,
    [ValorDuracion] nvarchar(max) NULL,
    [TipoDuracionRetardo] nvarchar(max) NOT NULL,
    [ValorDuracionRetardo] nvarchar(max) NULL,
    [Secuencia] int NOT NULL,
    [Link] nvarchar(max) NULL,
    CONSTRAINT [PK_Etapas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Etapas_Procesos_ProcesoId] FOREIGN KEY ([ProcesoId]) REFERENCES [Procesos] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Solicitudes] (
    [Id] int NOT NULL IDENTITY,
    [NumeroTicket] nvarchar(450) NOT NULL,
    [ProcesoId] int NULL,
    [Estado] nvarchar(200) NOT NULL,
    [Resumen] nvarchar(max) NULL,
    [InstanciadoPor] nvarchar(450) NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaTermino] datetime2 NULL,
    CONSTRAINT [PK_Solicitudes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Solicitudes_Procesos_ProcesoId] FOREIGN KEY ([ProcesoId]) REFERENCES [Procesos] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Comunas] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [RegionId] int NULL,
    CONSTRAINT [PK_Comunas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comunas_Regiones_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [Regiones] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [NotificacionesRoles] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_NotificacionesRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NotificacionesRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [TareasAutomaticas] (
    [Id] int NOT NULL IDENTITY,
    [EtapaId] int NULL,
    [Descripcion] nvarchar(max) NULL,
    [EventoDisparador] int NOT NULL,
    [Namespace] nvarchar(max) NULL,
    [Clase] nvarchar(max) NULL,
    [Metodo] nvarchar(max) NULL,
    [Secuencia] int NOT NULL,
    CONSTRAINT [PK_TareasAutomaticas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TareasAutomaticas_Etapas_EtapaId] FOREIGN KEY ([EtapaId]) REFERENCES [Etapas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Transiciones] (
    [Id] int NOT NULL IDENTITY,
    [EtapaActaualId] int NOT NULL,
    [EtapaDestinoId] int NOT NULL,
    [NamespaceValidacion] nvarchar(max) NULL,
    [ClaseValidacion] nvarchar(max) NULL,
    [MetodoValidacion] nvarchar(max) NULL,
    CONSTRAINT [PK_Transiciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transiciones_Etapas_EtapaActaualId] FOREIGN KEY ([EtapaActaualId]) REFERENCES [Etapas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transiciones_Etapas_EtapaDestinoId] FOREIGN KEY ([EtapaDestinoId]) REFERENCES [Etapas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Tareas] (
    [Id] int NOT NULL IDENTITY,
    [SolicitudId] int NOT NULL,
    [EtapaId] int NULL,
    [AsignadoA] nvarchar(max) NULL,
    [ReasignadoA] nvarchar(max) NULL,
    [EjecutadoPor] nvarchar(max) NULL,
    [Estado] nvarchar(200) NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaTerminoEstimada] datetime2 NULL,
    [FechaTerminoFinal] datetime2 NULL,
    CONSTRAINT [PK_Tareas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tareas_Etapas_EtapaId] FOREIGN KEY ([EtapaId]) REFERENCES [Etapas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Tareas_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Notarias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [ComunaId] int NOT NULL,
    CONSTRAINT [PK_Notarias] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notarias_Comunas_ComunaId] FOREIGN KEY ([ComunaId]) REFERENCES [Comunas] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Oficinas] (
    [Id] int NOT NULL IDENTITY,
    [Codificacion] nvarchar(max) NULL,
    [Nombre] nvarchar(max) NULL,
    [ComunaId] int NULL,
    [OficinaProcesoId] int NOT NULL,
    CONSTRAINT [PK_Oficinas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Oficinas_Comunas_ComunaId] FOREIGN KEY ([ComunaId]) REFERENCES [Comunas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Oficinas_Oficinas_OficinaProcesoId] FOREIGN KEY ([OficinaProcesoId]) REFERENCES [Oficinas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [PacksNotarias] (
    [Id] int NOT NULL IDENTITY,
    [FechaEnvio] datetime2 NOT NULL,
    [NotariaEnvioId] int NOT NULL,
    [OficinaId] int NOT NULL,
    CONSTRAINT [PK_PacksNotarias] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PacksNotarias_Notarias_NotariaEnvioId] FOREIGN KEY ([NotariaEnvioId]) REFERENCES [Notarias] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PacksNotarias_Oficinas_OficinaId] FOREIGN KEY ([OficinaId]) REFERENCES [Oficinas] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Usuarios] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    [Identificador] nvarchar(max) NULL,
    [EstadoCuenta] int NOT NULL,
    [TokenRecuerdaAcceso] nvarchar(max) NULL,
    [Eliminado] bit NOT NULL,
    [Nombres] nvarchar(max) NULL,
    [OficinaId] int NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Usuarios_Oficinas_OficinaId] FOREIGN KEY ([OficinaId]) REFERENCES [Oficinas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [ExpedientesCreditos] (
    [Id] int NOT NULL IDENTITY,
    [FechaCreacion] datetime2 NOT NULL,
    [CreditoId] int NOT NULL,
    [TipoExpediente] int NOT NULL,
    [PackNotariaId] int NULL,
    CONSTRAINT [PK_ExpedientesCreditos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExpedientesCreditos_Creditos_CreditoId] FOREIGN KEY ([CreditoId]) REFERENCES [Creditos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExpedientesCreditos_PacksNotarias_PackNotariaId] FOREIGN KEY ([PackNotariaId]) REFERENCES [PacksNotarias] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [AccesosUsuarios] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AccesosUsuarios] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AccesosUsuarios_Usuarios_UserId] FOREIGN KEY ([UserId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [NotificacionesUsuarios] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_NotificacionesUsuarios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NotificacionesUsuarios_Usuarios_UserId] FOREIGN KEY ([UserId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [RolesUsuarios] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_RolesUsuarios] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_RolesUsuarios_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolesUsuarios_Usuarios_UserId] FOREIGN KEY ([UserId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [TokensUsuarios] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_TokensUsuarios] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_TokensUsuarios_Usuarios_UserId] FOREIGN KEY ([UserId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Documentos] (
    [Id] int NOT NULL IDENTITY,
    [Resumen] nvarchar(max) NULL,
    [Codificacion] nvarchar(max) NOT NULL,
    [TipoDocumento] nvarchar(max) NOT NULL,
    [ExpedienteCreditoId] int NOT NULL,
    CONSTRAINT [PK_Documentos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documentos_ExpedientesCreditos_ExpedienteCreditoId] FOREIGN KEY ([ExpedienteCreditoId]) REFERENCES [ExpedientesCreditos] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_AccesosUsuarios_UserId] ON [AccesosUsuarios] ([UserId]);

GO

CREATE INDEX [IX_Comunas_RegionId] ON [Comunas] ([RegionId]);

GO

CREATE INDEX [IX_Documentos_ExpedienteCreditoId] ON [Documentos] ([ExpedienteCreditoId]);

GO

CREATE INDEX [IX_Etapas_ProcesoId] ON [Etapas] ([ProcesoId]);

GO

CREATE INDEX [IX_ExpedientesCreditos_CreditoId] ON [ExpedientesCreditos] ([CreditoId]);

GO

CREATE INDEX [IX_ExpedientesCreditos_PackNotariaId] ON [ExpedientesCreditos] ([PackNotariaId]);

GO

CREATE INDEX [IX_Notarias_ComunaId] ON [Notarias] ([ComunaId]);

GO

CREATE INDEX [IX_NotificacionesRoles_RoleId] ON [NotificacionesRoles] ([RoleId]);

GO

CREATE INDEX [IX_NotificacionesUsuarios_UserId] ON [NotificacionesUsuarios] ([UserId]);

GO

CREATE INDEX [IX_Oficinas_ComunaId] ON [Oficinas] ([ComunaId]);

GO

CREATE INDEX [IX_Oficinas_OficinaProcesoId] ON [Oficinas] ([OficinaProcesoId]);

GO

CREATE INDEX [IX_Organizaciones_PadreId] ON [Organizaciones] ([PadreId]);

GO

CREATE INDEX [IX_PacksNotarias_NotariaEnvioId] ON [PacksNotarias] ([NotariaEnvioId]);

GO

CREATE INDEX [IX_PacksNotarias_OficinaId] ON [PacksNotarias] ([OficinaId]);

GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

GO

CREATE INDEX [IX_Roles_OrzanizacionId] ON [Roles] ([OrzanizacionId]);

GO

CREATE INDEX [IX_Roles_PadreId] ON [Roles] ([PadreId]);

GO

CREATE INDEX [IX_RolesUsuarios_RoleId] ON [RolesUsuarios] ([RoleId]);

GO

CREATE INDEX [IX_Solicitudes_InstanciadoPor] ON [Solicitudes] ([InstanciadoPor]);

GO

CREATE UNIQUE INDEX [IX_Solicitudes_NumeroTicket] ON [Solicitudes] ([NumeroTicket]);

GO

CREATE INDEX [IX_Solicitudes_ProcesoId] ON [Solicitudes] ([ProcesoId]);

GO

CREATE INDEX [IX_Tareas_EtapaId] ON [Tareas] ([EtapaId]);

GO

CREATE INDEX [IX_Tareas_SolicitudId] ON [Tareas] ([SolicitudId]);

GO

CREATE INDEX [IX_TareasAutomaticas_EtapaId] ON [TareasAutomaticas] ([EtapaId]);

GO

CREATE INDEX [IX_Transiciones_EtapaActaualId] ON [Transiciones] ([EtapaActaualId]);

GO

CREATE INDEX [IX_Transiciones_EtapaDestinoId] ON [Transiciones] ([EtapaDestinoId]);

GO

CREATE INDEX [EmailIndex] ON [Usuarios] ([NormalizedEmail]);

GO

CREATE UNIQUE INDEX [UserNameIndex] ON [Usuarios] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

GO

CREATE INDEX [IX_Usuarios_OficinaId] ON [Usuarios] ([OficinaId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181004144508_MigracionInicialSS', N'2.1.4-rtm-31024');

GO

ALTER TABLE [PacksNotarias] ADD [CodigoSeguimiento] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181009132844_AddSeguimientoPacks', N'2.1.4-rtm-31024');

GO

ALTER TABLE [ExpedientesCreditos] ADD [ValijaValoradaId] int NULL;

GO

CREATE TABLE [ValijasValoradas] (
    [Id] int NOT NULL IDENTITY,
    [FechaEnvio] datetime2 NOT NULL,
    [OficinaId] int NULL,
    [CodigoSeguimiento] nvarchar(max) NULL,
    CONSTRAINT [PK_ValijasValoradas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ValijasValoradas_Oficinas_OficinaId] FOREIGN KEY ([OficinaId]) REFERENCES [Oficinas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_ExpedientesCreditos_ValijaValoradaId] ON [ExpedientesCreditos] ([ValijaValoradaId]);

GO

CREATE INDEX [IX_ValijasValoradas_OficinaId] ON [ValijasValoradas] ([OficinaId]);

GO

ALTER TABLE [ExpedientesCreditos] ADD CONSTRAINT [FK_ExpedientesCreditos_ValijasValoradas_ValijaValoradaId] FOREIGN KEY ([ValijaValoradaId]) REFERENCES [ValijasValoradas] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181011200404_ValijaValoradaNueva', N'2.1.4-rtm-31024');

GO

ALTER TABLE [ValijasValoradas] ADD [MarcaAvance] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181012143901_MarcaAvanceField', N'2.1.4-rtm-31024');

GO

ALTER TABLE [ExpedientesCreditos] ADD [CajaValoradaId] int NULL;

GO

CREATE TABLE [CajasValoradas] (
    [Id] int NOT NULL IDENTITY,
    [FechaEnvio] datetime2 NOT NULL,
    [CodigoSeguimiento] nvarchar(max) NULL,
    [MarcaAvance] nvarchar(max) NULL,
    CONSTRAINT [PK_CajasValoradas] PRIMARY KEY ([Id])
);

GO

CREATE INDEX [IX_ExpedientesCreditos_CajaValoradaId] ON [ExpedientesCreditos] ([CajaValoradaId]);

GO

ALTER TABLE [ExpedientesCreditos] ADD CONSTRAINT [FK_ExpedientesCreditos_CajasValoradas_CajaValoradaId] FOREIGN KEY ([CajaValoradaId]) REFERENCES [CajasValoradas] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181017213046_NuevaCajaValorada', N'2.1.4-rtm-31024');

GO

ALTER TABLE [ExpedientesCreditos] ADD [ValijaOficinaId] int NULL;

GO

CREATE TABLE [ValijasOficinas] (
    [Id] int NOT NULL IDENTITY,
    [FechaEnvio] datetime2 NOT NULL,
    [OficinaEnvioId] int NULL,
    [OficinaDestinoId] int NULL,
    [CodigoSeguimiento] nvarchar(max) NULL,
    [MarcaAvance] nvarchar(max) NULL,
    CONSTRAINT [PK_ValijasOficinas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ValijasOficinas_Oficinas_OficinaDestinoId] FOREIGN KEY ([OficinaDestinoId]) REFERENCES [Oficinas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ValijasOficinas_Oficinas_OficinaEnvioId] FOREIGN KEY ([OficinaEnvioId]) REFERENCES [Oficinas] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_ExpedientesCreditos_ValijaOficinaId] ON [ExpedientesCreditos] ([ValijaOficinaId]);

GO

CREATE INDEX [IX_ValijasOficinas_OficinaDestinoId] ON [ValijasOficinas] ([OficinaDestinoId]);

GO

CREATE INDEX [IX_ValijasOficinas_OficinaEnvioId] ON [ValijasOficinas] ([OficinaEnvioId]);

GO

ALTER TABLE [ExpedientesCreditos] ADD CONSTRAINT [FK_ExpedientesCreditos_ValijasOficinas_ValijaOficinaId] FOREIGN KEY ([ValijaOficinaId]) REFERENCES [ValijasOficinas] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181022193429_ValijaOficinasEnt', N'2.1.4-rtm-31024');

GO

ALTER TABLE [Tareas] ADD [UnidadNegocioAsignada] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181025151625_UnidadNegocioTarea', N'2.1.4-rtm-31024');

GO

ALTER TABLE [Etapas] ADD [UnidadNegocioAsignar] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181025153352_UnidadNegocioAsignarEtapa', N'2.1.4-rtm-31024');

GO

