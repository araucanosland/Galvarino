DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tareas]') AND [c].[name] = N'UnidadNegocioAsignada');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Tareas] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Tareas] ALTER COLUMN [UnidadNegocioAsignada] nvarchar(450) NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tareas]') AND [c].[name] = N'AsignadoA');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Tareas] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Tareas] ALTER COLUMN [AsignadoA] nvarchar(450) NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etapas]') AND [c].[name] = N'TipoUsuarioAsignado');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Etapas] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Etapas] ALTER COLUMN [TipoUsuarioAsignado] nvarchar(450) NOT NULL;

GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etapas]') AND [c].[name] = N'TipoEtapa');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Etapas] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Etapas] ALTER COLUMN [TipoEtapa] nvarchar(450) NOT NULL;

GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Etapas]') AND [c].[name] = N'NombreInterno');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Etapas] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Etapas] ALTER COLUMN [NombreInterno] nvarchar(450) NULL;

GO

CREATE INDEX [IX_Tareas_AsignadoA_Estado_UnidadNegocioAsignada] ON [Tareas] ([AsignadoA], [Estado], [UnidadNegocioAsignada]);

GO

CREATE INDEX [IX_Etapas_NombreInterno_TipoEtapa_TipoUsuarioAsignado] ON [Etapas] ([NombreInterno], [TipoEtapa], [TipoUsuarioAsignado]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181212182033_IndexesPt1', N'2.1.4-rtm-31024');

GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Creditos]') AND [c].[name] = N'NumeroTicket');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Creditos] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Creditos] ALTER COLUMN [NumeroTicket] nvarchar(450) NULL;

GO

CREATE INDEX [IX_Creditos_NumeroTicket] ON [Creditos] ([NumeroTicket]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181212183016_IndexesPt2', N'2.1.4-rtm-31024');

GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Variables]') AND [c].[name] = N'NumeroTicket');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Variables] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Variables] ALTER COLUMN [NumeroTicket] nvarchar(450) NULL;

GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Variables]') AND [c].[name] = N'Clave');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Variables] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Variables] ALTER COLUMN [Clave] nvarchar(450) NULL;

GO

CREATE INDEX [IX_Variables_NumeroTicket_Clave] ON [Variables] ([NumeroTicket], [Clave]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181212183701_IndexesPt3', N'2.1.4-rtm-31024');

GO

