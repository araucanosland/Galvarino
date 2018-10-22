﻿// <auto-generated />
using System;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Galvarino.Web.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Galvarino.Web.Models.Application.CajaValorada", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodigoSeguimiento");

                    b.Property<DateTime>("FechaEnvio");

                    b.Property<string>("MarcaAvance");

                    b.HasKey("Id");

                    b.ToTable("CajasValoradas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.CargaInicial", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CanalVenta");

                    b.Property<string>("CodigoOficinaIngreso");

                    b.Property<string>("CodigoOficinaPago");

                    b.Property<string>("Estado");

                    b.Property<DateTime>("FechaCarga");

                    b.Property<DateTime>("FechaCorresponde");

                    b.Property<string>("FechaVigencia");

                    b.Property<string>("FolioCredito")
                        .IsRequired();

                    b.Property<string>("LineaCredito");

                    b.Property<string>("NombreArchivoCarga");

                    b.Property<string>("RutAfiliado");

                    b.Property<string>("RutResponsable");

                    b.HasKey("Id");

                    b.ToTable("CargasIniciales");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Comuna", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Nombre");

                    b.Property<int?>("RegionId");

                    b.HasKey("Id");

                    b.HasIndex("RegionId");

                    b.ToTable("Comunas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ConfiguracionDocumento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("TipoCredito");

                    b.Property<int>("TipoDocumento");

                    b.Property<int>("TipoExpediente");

                    b.HasKey("Id");

                    b.ToTable("ConfiguracionDocumentos");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Credito", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("FechaDesembolso");

                    b.Property<DateTime>("FechaFormaliza");

                    b.Property<string>("FolioCredito");

                    b.Property<long>("MontoCredito");

                    b.Property<string>("NombreCliente");

                    b.Property<string>("NumeroTicket");

                    b.Property<string>("RutCliente");

                    b.Property<int>("TipoCredito");

                    b.HasKey("Id");

                    b.ToTable("Creditos");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Documento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Codificacion")
                        .IsRequired();

                    b.Property<int?>("ExpedienteCreditoId")
                        .IsRequired();

                    b.Property<string>("Resumen");

                    b.Property<string>("TipoDocumento")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ExpedienteCreditoId");

                    b.ToTable("Documentos");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ExpedienteCredito", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CajaValoradaId");

                    b.Property<int?>("CreditoId")
                        .IsRequired();

                    b.Property<DateTime>("FechaCreacion");

                    b.Property<int?>("PackNotariaId");

                    b.Property<int>("TipoExpediente");

                    b.Property<int?>("ValijaOficinaId");

                    b.Property<int?>("ValijaValoradaId");

                    b.HasKey("Id");

                    b.HasIndex("CajaValoradaId");

                    b.HasIndex("CreditoId");

                    b.HasIndex("PackNotariaId");

                    b.HasIndex("ValijaOficinaId");

                    b.HasIndex("ValijaValoradaId");

                    b.ToTable("ExpedientesCreditos");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Notaria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ComunaId")
                        .IsRequired();

                    b.Property<string>("Nombre");

                    b.HasKey("Id");

                    b.HasIndex("ComunaId");

                    b.ToTable("Notarias");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Oficina", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Codificacion");

                    b.Property<int?>("ComunaId");

                    b.Property<string>("Nombre");

                    b.Property<int?>("OficinaProcesoId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ComunaId");

                    b.HasIndex("OficinaProcesoId");

                    b.ToTable("Oficinas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.PackNotaria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodigoSeguimiento");

                    b.Property<DateTime>("FechaEnvio");

                    b.Property<int?>("NotariaEnvioId")
                        .IsRequired();

                    b.Property<int?>("OficinaId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("NotariaEnvioId");

                    b.HasIndex("OficinaId");

                    b.ToTable("PacksNotarias");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Nombre");

                    b.Property<int>("Secuencia");

                    b.HasKey("Id");

                    b.ToTable("Regiones");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ValijaOficina", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodigoSeguimiento");

                    b.Property<DateTime>("FechaEnvio");

                    b.Property<string>("MarcaAvance");

                    b.Property<int?>("OficinaDestinoId");

                    b.Property<int?>("OficinaEnvioId");

                    b.HasKey("Id");

                    b.HasIndex("OficinaDestinoId");

                    b.HasIndex("OficinaEnvioId");

                    b.ToTable("ValijasOficinas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ValijaValorada", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodigoSeguimiento");

                    b.Property<DateTime>("FechaEnvio");

                    b.Property<string>("MarcaAvance");

                    b.Property<int?>("OficinaId");

                    b.HasKey("Id");

                    b.HasIndex("OficinaId");

                    b.ToTable("ValijasValoradas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Organizacion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Nombre")
                        .IsRequired();

                    b.Property<int?>("PadreId");

                    b.Property<string>("TipoOrganizacion")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PadreId");

                    b.ToTable("Organizaciones");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Rol", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Activo");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.Property<int?>("OrzanizacionId")
                        .IsRequired();

                    b.Property<string>("PadreId");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.HasIndex("OrzanizacionId");

                    b.HasIndex("PadreId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Usuario", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<bool>("Eliminado");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<int>("EstadoCuenta");

                    b.Property<string>("Identificador");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("Nombres");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<int?>("OficinaId");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("TokenRecuerdaAcceso");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("OficinaId");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Etapa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Link");

                    b.Property<string>("Nombre");

                    b.Property<string>("NombreInterno");

                    b.Property<int?>("ProcesoId")
                        .IsRequired();

                    b.Property<int>("Secuencia");

                    b.Property<string>("TipoDuracion")
                        .IsRequired();

                    b.Property<string>("TipoDuracionRetardo")
                        .IsRequired();

                    b.Property<string>("TipoEtapa")
                        .IsRequired();

                    b.Property<string>("TipoUsuarioAsignado")
                        .IsRequired();

                    b.Property<string>("ValorDuracion");

                    b.Property<string>("ValorDuracionRetardo");

                    b.Property<string>("ValorUsuarioAsignado")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ProcesoId");

                    b.ToTable("Etapas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Proceso", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Activo");

                    b.Property<string>("ClaseGeneraTickets");

                    b.Property<string>("MetodoGeneraTickets");

                    b.Property<string>("NamespaceGeneraTickets");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(150);

                    b.Property<string>("NombreInterno");

                    b.HasKey("Id");

                    b.ToTable("Procesos");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Solicitud", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTime>("FechaInicio");

                    b.Property<DateTime?>("FechaTermino");

                    b.Property<string>("InstanciadoPor")
                        .IsRequired();

                    b.Property<string>("NumeroTicket")
                        .IsRequired();

                    b.Property<int?>("ProcesoId");

                    b.Property<string>("Resumen");

                    b.HasKey("Id");

                    b.HasIndex("InstanciadoPor");

                    b.HasIndex("NumeroTicket")
                        .IsUnique();

                    b.HasIndex("ProcesoId");

                    b.ToTable("Solicitudes");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Tarea", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AsignadoA");

                    b.Property<string>("EjecutadoPor");

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int?>("EtapaId");

                    b.Property<DateTime>("FechaInicio");

                    b.Property<DateTime?>("FechaTerminoEstimada");

                    b.Property<DateTime?>("FechaTerminoFinal");

                    b.Property<string>("ReasignadoA");

                    b.Property<int?>("SolicitudId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("EtapaId");

                    b.HasIndex("SolicitudId");

                    b.ToTable("Tareas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.TareaAutomatica", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Clase");

                    b.Property<string>("Descripcion");

                    b.Property<int?>("EtapaId");

                    b.Property<int>("EventoDisparador");

                    b.Property<string>("Metodo");

                    b.Property<string>("Namespace");

                    b.Property<int>("Secuencia");

                    b.HasKey("Id");

                    b.HasIndex("EtapaId");

                    b.ToTable("TareasAutomaticas");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Transito", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaseValidacion");

                    b.Property<int?>("EtapaActaualId")
                        .IsRequired();

                    b.Property<int?>("EtapaDestinoId")
                        .IsRequired();

                    b.Property<string>("MetodoValidacion");

                    b.Property<string>("NamespaceValidacion");

                    b.HasKey("Id");

                    b.HasIndex("EtapaActaualId");

                    b.HasIndex("EtapaDestinoId");

                    b.ToTable("Transiciones");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Variable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Clave");

                    b.Property<string>("NumeroTicket");

                    b.Property<string>("Tipo");

                    b.Property<string>("Valor");

                    b.HasKey("Id");

                    b.ToTable("Variables");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("NotificacionesRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("NotificacionesUsuarios");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AccesosUsuarios");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolesUsuarios");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("TokensUsuarios");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Comuna", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Region", "Region")
                        .WithMany("Comunas")
                        .HasForeignKey("RegionId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Documento", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.ExpedienteCredito", "ExpedienteCredito")
                        .WithMany("Documentos")
                        .HasForeignKey("ExpedienteCreditoId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ExpedienteCredito", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.CajaValorada", "CajaValorada")
                        .WithMany("Expedientes")
                        .HasForeignKey("CajaValoradaId");

                    b.HasOne("Galvarino.Web.Models.Application.Credito", "Credito")
                        .WithMany()
                        .HasForeignKey("CreditoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Galvarino.Web.Models.Application.PackNotaria", "PackNotaria")
                        .WithMany("Expedientes")
                        .HasForeignKey("PackNotariaId");

                    b.HasOne("Galvarino.Web.Models.Application.ValijaOficina")
                        .WithMany("Expedientes")
                        .HasForeignKey("ValijaOficinaId");

                    b.HasOne("Galvarino.Web.Models.Application.ValijaValorada", "ValijaValorada")
                        .WithMany("Expedientes")
                        .HasForeignKey("ValijaValoradaId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Notaria", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Comuna", "Comuna")
                        .WithMany()
                        .HasForeignKey("ComunaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.Oficina", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Comuna", "Comuna")
                        .WithMany()
                        .HasForeignKey("ComunaId");

                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "OficinaProceso")
                        .WithMany()
                        .HasForeignKey("OficinaProcesoId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.PackNotaria", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Notaria", "NotariaEnvio")
                        .WithMany()
                        .HasForeignKey("NotariaEnvioId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "Oficina")
                        .WithMany("PacksNotaria")
                        .HasForeignKey("OficinaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ValijaOficina", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "OficinaDestino")
                        .WithMany()
                        .HasForeignKey("OficinaDestinoId");

                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "OficinaEnvio")
                        .WithMany()
                        .HasForeignKey("OficinaEnvioId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Application.ValijaValorada", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "Oficina")
                        .WithMany()
                        .HasForeignKey("OficinaId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Organizacion", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Organizacion", "Padre")
                        .WithMany("Hijos")
                        .HasForeignKey("PadreId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Rol", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Organizacion", "Orzanizacion")
                        .WithMany("Roles")
                        .HasForeignKey("OrzanizacionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Galvarino.Web.Models.Security.Rol", "Padre")
                        .WithMany("Hijos")
                        .HasForeignKey("PadreId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Security.Usuario", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Application.Oficina", "Oficina")
                        .WithMany()
                        .HasForeignKey("OficinaId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Etapa", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Workflow.Proceso", "Proceso")
                        .WithMany("Etapas")
                        .HasForeignKey("ProcesoId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Solicitud", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Workflow.Proceso", "Proceso")
                        .WithMany("Solicitudes")
                        .HasForeignKey("ProcesoId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Tarea", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Workflow.Etapa", "Etapa")
                        .WithMany("Tareas")
                        .HasForeignKey("EtapaId");

                    b.HasOne("Galvarino.Web.Models.Workflow.Solicitud", "Solicitud")
                        .WithMany("Tareas")
                        .HasForeignKey("SolicitudId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.TareaAutomatica", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Workflow.Etapa", "Etapa")
                        .WithMany("TareasAutomaticas")
                        .HasForeignKey("EtapaId");
                });

            modelBuilder.Entity("Galvarino.Web.Models.Workflow.Transito", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Workflow.Etapa", "EtapaActaual")
                        .WithMany("Actuales")
                        .HasForeignKey("EtapaActaualId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Galvarino.Web.Models.Workflow.Etapa", "EtapaDestino")
                        .WithMany("Destinos")
                        .HasForeignKey("EtapaDestinoId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Rol")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Usuario")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Usuario")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Rol")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Galvarino.Web.Models.Security.Usuario")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Galvarino.Web.Models.Security.Usuario")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
