using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Galvarino.Workflow.Model;
using Galvarino.Workflow.Infrastructure.Configurtations;

namespace Galvarino.Workflow.Infrastructure
{
    public class WorkflowDbContext : DbContext
    {
        public DbSet<Etapa> Etapas { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<TareaAutomatica> TareasAutomaticas { get; set; }
        public DbSet<Transito> Transiciones { get; set; }
        public DbSet<Variable> Variables { get; set; }


        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }

        public WorkflowDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.HasDefaultSchema("workflow");
            builder.ApplyConfiguration(new ProcesoConfig());
            builder.ApplyConfiguration(new EtapaConfig());
            builder.ApplyConfiguration(new SolicitudConfig());
            builder.ApplyConfiguration(new TareaConfig());
            builder.ApplyConfiguration(new VariablesConfig());
        }
    }
}
