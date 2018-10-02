using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class MigracionInicialPackCompleto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargasIniciales",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaCarga = table.Column<DateTime>(nullable: false),
                    FechaCorresponde = table.Column<DateTime>(nullable: false),
                    FolioCredito = table.Column<string>(nullable: false),
                    RutAfiliado = table.Column<string>(nullable: true),
                    CodigoOficinaIngreso = table.Column<string>(nullable: true),
                    CodigoOficinaPago = table.Column<string>(nullable: true),
                    LineaCredito = table.Column<string>(nullable: true),
                    RutResponsable = table.Column<string>(nullable: true),
                    CanalVenta = table.Column<string>(nullable: true),
                    Estado = table.Column<string>(nullable: true),
                    FechaVigencia = table.Column<string>(nullable: true),
                    NombreArchivoCarga = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargasIniciales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TipoDocumento = table.Column<int>(nullable: false),
                    TipoExpediente = table.Column<int>(nullable: false),
                    TipoCredito = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionDocumentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Creditos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FolioCredito = table.Column<string>(nullable: true),
                    MontoCredito = table.Column<long>(nullable: false),
                    FechaFormaliza = table.Column<DateTime>(nullable: false),
                    FechaDesembolso = table.Column<DateTime>(nullable: false),
                    RutCliente = table.Column<string>(nullable: true),
                    NombreCliente = table.Column<string>(nullable: true),
                    NumeroTicket = table.Column<string>(nullable: true),
                    TipoCredito = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creditos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: false),
                    TipoOrganizacion = table.Column<string>(nullable: false),
                    PadreId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizaciones_Organizaciones_PadreId",
                        column: x => x.PadreId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Procesos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(maxLength: 150, nullable: false),
                    NombreInterno = table.Column<string>(nullable: true),
                    NamespaceGeneraTickets = table.Column<string>(nullable: true),
                    ClaseGeneraTickets = table.Column<string>(nullable: true),
                    MetodoGeneraTickets = table.Column<string>(nullable: true),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procesos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regiones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true),
                    Secuencia = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroTicket = table.Column<string>(nullable: true),
                    Clave = table.Column<string>(nullable: true),
                    Valor = table.Column<string>(nullable: true),
                    Tipo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    OrzanizacionId = table.Column<int>(nullable: false),
                    PadreId = table.Column<string>(nullable: true),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Organizaciones_OrzanizacionId",
                        column: x => x.OrzanizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_Roles_PadreId",
                        column: x => x.PadreId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Etapas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProcesoId = table.Column<int>(nullable: false),
                    TipoEtapa = table.Column<string>(nullable: false),
                    Nombre = table.Column<string>(nullable: true),
                    NombreInterno = table.Column<string>(nullable: true),
                    TipoUsuarioAsignado = table.Column<string>(nullable: false),
                    ValorUsuarioAsignado = table.Column<string>(nullable: false),
                    TipoDuracion = table.Column<string>(nullable: false),
                    ValorDuracion = table.Column<string>(nullable: true),
                    TipoDuracionRetardo = table.Column<string>(nullable: false),
                    ValorDuracionRetardo = table.Column<string>(nullable: true),
                    Secuencia = table.Column<int>(nullable: false),
                    Link = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etapas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Etapas_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroTicket = table.Column<string>(nullable: false),
                    ProcesoId = table.Column<int>(nullable: true),
                    Estado = table.Column<string>(maxLength: 200, nullable: false),
                    Resumen = table.Column<string>(nullable: true),
                    InstanciadoPor = table.Column<string>(nullable: false),
                    FechaInicio = table.Column<DateTime>(nullable: false),
                    FechaTermino = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comunas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true),
                    RegionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comunas_Regiones_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacionesRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TareasAutomaticas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtapaId = table.Column<int>(nullable: true),
                    Descripcion = table.Column<string>(nullable: true),
                    EventoDisparador = table.Column<int>(nullable: false),
                    Namespace = table.Column<string>(nullable: true),
                    Clase = table.Column<string>(nullable: true),
                    Metodo = table.Column<string>(nullable: true),
                    Secuencia = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasAutomaticas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareasAutomaticas_Etapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "Etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transiciones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtapaActaualId = table.Column<int>(nullable: false),
                    EtapaDestinoId = table.Column<int>(nullable: false),
                    NamespaceValidacion = table.Column<string>(nullable: true),
                    ClaseValidacion = table.Column<string>(nullable: true),
                    MetodoValidacion = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transiciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transiciones_Etapas_EtapaActaualId",
                        column: x => x.EtapaActaualId,
                        principalTable: "Etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transiciones_Etapas_EtapaDestinoId",
                        column: x => x.EtapaDestinoId,
                        principalTable: "Etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SolicitudId = table.Column<int>(nullable: false),
                    EtapaId = table.Column<int>(nullable: true),
                    AsignadoA = table.Column<string>(nullable: true),
                    ReasignadoA = table.Column<string>(nullable: true),
                    EjecutadoPor = table.Column<string>(nullable: true),
                    Estado = table.Column<string>(maxLength: 200, nullable: false),
                    FechaInicio = table.Column<DateTime>(nullable: false),
                    FechaTerminoEstimada = table.Column<DateTime>(nullable: true),
                    FechaTerminoFinal = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Etapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "Etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notarias",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true),
                    ComunaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notarias_Comunas_ComunaId",
                        column: x => x.ComunaId,
                        principalTable: "Comunas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oficinas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codificacion = table.Column<string>(nullable: true),
                    Nombre = table.Column<string>(nullable: true),
                    ComunaId = table.Column<int>(nullable: true),
                    OficinaProcesoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oficinas_Comunas_ComunaId",
                        column: x => x.ComunaId,
                        principalTable: "Comunas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oficinas_Oficinas_OficinaProcesoId",
                        column: x => x.OficinaProcesoId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PacksNotarias",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaEnvio = table.Column<DateTime>(nullable: false),
                    NotariaEnvioId = table.Column<int>(nullable: false),
                    OficinaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacksNotarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacksNotarias_Notarias_NotariaEnvioId",
                        column: x => x.NotariaEnvioId,
                        principalTable: "Notarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PacksNotarias_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Identificador = table.Column<string>(nullable: true),
                    EstadoCuenta = table.Column<int>(nullable: false),
                    TokenRecuerdaAcceso = table.Column<string>(nullable: true),
                    Eliminado = table.Column<bool>(nullable: false),
                    Nombres = table.Column<string>(nullable: true),
                    OficinaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpedientesCreditos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaCreacion = table.Column<DateTime>(nullable: false),
                    CreditoId = table.Column<int>(nullable: false),
                    TipoExpediente = table.Column<int>(nullable: false),
                    PackNotariaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpedientesCreditos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpedientesCreditos_Creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "Creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpedientesCreditos_PacksNotarias_PackNotariaId",
                        column: x => x.PackNotariaId,
                        principalTable: "PacksNotarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccesosUsuarios",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesosUsuarios", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AccesosUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacionesUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolesUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesUsuarios", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RolesUsuarios_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokensUsuarios",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensUsuarios", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_TokensUsuarios_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Resumen = table.Column<string>(nullable: true),
                    Codificacion = table.Column<string>(nullable: false),
                    TipoDocumento = table.Column<string>(nullable: false),
                    ExpedienteCreditoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_ExpedientesCreditos_ExpedienteCreditoId",
                        column: x => x.ExpedienteCreditoId,
                        principalTable: "ExpedientesCreditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccesosUsuarios_UserId",
                table: "AccesosUsuarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comunas_RegionId",
                table: "Comunas",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ExpedienteCreditoId",
                table: "Documentos",
                column: "ExpedienteCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_Etapas_ProcesoId",
                table: "Etapas",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_CreditoId",
                table: "ExpedientesCreditos",
                column: "CreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_PackNotariaId",
                table: "ExpedientesCreditos",
                column: "PackNotariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notarias_ComunaId",
                table: "Notarias",
                column: "ComunaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesRoles_RoleId",
                table: "NotificacionesRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesUsuarios_UserId",
                table: "NotificacionesUsuarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_ComunaId",
                table: "Oficinas",
                column: "ComunaId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_OficinaProcesoId",
                table: "Oficinas",
                column: "OficinaProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizaciones_PadreId",
                table: "Organizaciones",
                column: "PadreId");

            migrationBuilder.CreateIndex(
                name: "IX_PacksNotarias_NotariaEnvioId",
                table: "PacksNotarias",
                column: "NotariaEnvioId");

            migrationBuilder.CreateIndex(
                name: "IX_PacksNotarias_OficinaId",
                table: "PacksNotarias",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrzanizacionId",
                table: "Roles",
                column: "OrzanizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PadreId",
                table: "Roles",
                column: "PadreId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesUsuarios_RoleId",
                table: "RolesUsuarios",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_InstanciadoPor",
                table: "Solicitudes",
                column: "InstanciadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_NumeroTicket",
                table: "Solicitudes",
                column: "NumeroTicket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_ProcesoId",
                table: "Solicitudes",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_EtapaId",
                table: "Tareas",
                column: "EtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_SolicitudId",
                table: "Tareas",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_TareasAutomaticas_EtapaId",
                table: "TareasAutomaticas",
                column: "EtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transiciones_EtapaActaualId",
                table: "Transiciones",
                column: "EtapaActaualId");

            migrationBuilder.CreateIndex(
                name: "IX_Transiciones_EtapaDestinoId",
                table: "Transiciones",
                column: "EtapaDestinoId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_OficinaId",
                table: "Usuarios",
                column: "OficinaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccesosUsuarios");

            migrationBuilder.DropTable(
                name: "CargasIniciales");

            migrationBuilder.DropTable(
                name: "ConfiguracionDocumentos");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "NotificacionesRoles");

            migrationBuilder.DropTable(
                name: "NotificacionesUsuarios");

            migrationBuilder.DropTable(
                name: "RolesUsuarios");

            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "TareasAutomaticas");

            migrationBuilder.DropTable(
                name: "TokensUsuarios");

            migrationBuilder.DropTable(
                name: "Transiciones");

            migrationBuilder.DropTable(
                name: "Variables");

            migrationBuilder.DropTable(
                name: "ExpedientesCreditos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Etapas");

            migrationBuilder.DropTable(
                name: "Creditos");

            migrationBuilder.DropTable(
                name: "PacksNotarias");

            migrationBuilder.DropTable(
                name: "Organizaciones");

            migrationBuilder.DropTable(
                name: "Procesos");

            migrationBuilder.DropTable(
                name: "Notarias");

            migrationBuilder.DropTable(
                name: "Oficinas");

            migrationBuilder.DropTable(
                name: "Comunas");

            migrationBuilder.DropTable(
                name: "Regiones");
        }
    }
}
