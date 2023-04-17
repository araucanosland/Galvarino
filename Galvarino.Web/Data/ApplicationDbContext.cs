using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Data.Configurtations.Security;
using Galvarino.Web.Data.Configurtations.Workflow;
using Galvarino.Web.Data.Configurtations.Application;
using Microsoft.Extensions.Configuration;
using Galvarino.Web.Models.Application.Pensionado;

namespace Galvarino.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, string>
    {
        public DbSet<Organizacion> Organizaciones { get; set; }

        public DbSet<Etapa> Etapas { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<TareaAutomatica> TareasAutomaticas { get; set; }
        public DbSet<Transito> Transiciones { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<Oficina> Oficinas { get; set; }



        #region Creditos

        public DbSet<CargaInicial> CargasIniciales { get; set; }
        public DbSet<Comuna> Comunas { get; set; }
        public DbSet<Credito> Creditos { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<ExpedienteCredito> ExpedientesCreditos { get; set; }
        public DbSet<Notaria> Notarias { get; set; }
        public DbSet<PackNotaria> PacksNotarias { get; set; }
        public DbSet<ValijaValorada> ValijasValoradas { get; set; }
        public DbSet<ValijaOficina> ValijasOficinas { get; set; }
        public DbSet<CajaValorada> CajasValoradas { get; set; }
        public DbSet<Region> Regiones { get; set; }
        public DbSet<ConfiguracionDocumento> ConfiguracionDocumentos { get; set; }
        public DbSet<AlmacenajeComercial> AlmacenajesComerciales { get; set; }
        public DbSet<AuditorReasignacion> AudicionesReasignaciones { get; set; }
        public DbSet<PagareSinCustodia> PagaresSinCustodia { get; set; }
        public DbSet<GestionPagareSinCustodia> GestionPagaresSinCustodia { get; set; }
        public DbSet<PasoValijaValorada> PasosValijasValoradas { get; set; }

        public DbSet<ReporteProgramado> ReporteProgramado { get; set; }


        #endregion

       


        private readonly IConfiguration _conf;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration conf)
            : base(options)
        {
            _conf = conf;
        }

        public ApplicationDbContext(IConfiguration conf)
        {
            _conf = conf;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.HasDefaultSchema(_conf.GetValue<string>("schema"));

            /*Identity*/
            builder.Entity<Usuario>().ToTable("Usuarios");
            builder.Entity<Rol>().ToTable("Roles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("NotificacionesUsuarios");
            builder.Entity<IdentityUserRole<string>>().ToTable("RolesUsuarios");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AccesosUsuarios");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("NotificacionesRoles");
            builder.Entity<IdentityUserToken<string>>().ToTable("TokensUsuarios");

            /*Configurar Modelos*/
            builder.ApplyConfiguration(new OrganizacionConfig());
            builder.ApplyConfiguration(new ProcesoConfig());
            builder.ApplyConfiguration(new EtapaConfig());
            builder.ApplyConfiguration(new SolicitudConfig());
            builder.ApplyConfiguration(new TareaConfig());
            builder.ApplyConfiguration(new CargaInicialConfig());
            builder.ApplyConfiguration(new DocumentoConfig());
            builder.ApplyConfiguration(new ExpedienteCreditoConfig());
            builder.ApplyConfiguration(new NotariaConfig());
            builder.ApplyConfiguration(new OficinaConfig());
            builder.ApplyConfiguration(new PackNotariaConfig());
            builder.ApplyConfiguration(new RegionConfig());
            builder.ApplyConfiguration(new ValijaValoradaConfig());
            builder.ApplyConfiguration(new CajaValoradaConfig());
            builder.ApplyConfiguration(new CreditoConfig());
            builder.ApplyConfiguration(new VariablesConfig());

            

        }
    }
}