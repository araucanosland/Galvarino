using Galvarino.Web.Data;
using Galvarino.Web.Hubs;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Services;
using Galvarino.Web.Services.Workflow;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galvarino.Web.Workers
{
    internal class CierreCajaValoradaWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificacionCajaCerradaHub> _notificion;
        private readonly IServiceScope _scope;
        private IWorkflowService _wfservice;
        
        public CierreCajaValoradaWorker(ILogger<CierreCajaValoradaWorker> logger, IServiceProvider services, IConfiguration configuration, IHubContext<NotificacionCajaCerradaHub> notificion)
        {
            _logger = logger;
            _notificion = notificion;
            _configuration = configuration;
            _scope = services.CreateScope();

            

        }
       
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                    .Options;

            var _context = new ApplicationDbContext(options, _configuration);
            //var _context = _scope.ServiceProvider.GetService<ApplicationDbContext>();
            //_timer = new Timer(DoWork, _context, TimeSpan.Zero,TimeSpan.FromSeconds(15));
            _wfservice = new WorkflowService(new DefaultWorkflowKernel(_context, _configuration));
            
            while (!cancellationToken.IsCancellationRequested)
            {
              
                _logger.LogInformation("Vuelta!!");
                var caja = await _context.CajasValoradas.OrderBy(d => d.CodigoSeguimiento).AsNoTracking().FirstOrDefaultAsync(d => d.MarcaAvance == "READYTOPROCESS-NOT-YET");

                if (caja != null)
                {
                    caja.MarcaAvance = "INPROCCES";
                    _context.CajasValoradas.Update(caja);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Agarramos la caja: " + caja.CodigoSeguimiento);


                    var transa = _context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {

                        List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                        List<string> ticketsAvanzar = new List<string>();
                        var folios = _context.PasosValijasValoradas.Where(c => c.CodigoCajaValorada == caja.CodigoSeguimiento).GroupBy(d => d.FolioCredito).Select(d => d.Key).ToList();

                        foreach (var item in folios)
                        {
                            var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item);
                            elExpediente.CajaValorada = caja;
                            expedientesModificados.Add(elExpediente);
                            ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                        }

                        _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                        await _context.SaveChangesAsync();
                        await _wfservice.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA, ticketsAvanzar, caja.Usuario);

                        /*Delete entries form box*/
                        _context.PasosValijasValoradas.RemoveRange(_context.PasosValijasValoradas.Where(c => c.CodigoCajaValorada == caja.CodigoSeguimiento && c.Usuario == caja.Usuario));


                        caja.MarcaAvance = "DESPACUST";
                        _context.CajasValoradas.Update(caja);
                        await _context.SaveChangesAsync();

                        var usr = _context.Users.FirstOrDefault(du => du.Identificador == caja.Usuario);
                        await _notificion.Clients.User(usr.Id).SendAsync("NotificarCajaCerrada", caja.CodigoSeguimiento);

                        transa.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Transa lanzo error. " + ex.Message);
                        transa.Rollback();

                        if (caja != null)
                        {
                            caja.MarcaAvance = "READYTOPROCESS-NOT-YET";
                            _context.CajasValoradas.Update(caja);
                            _context.SaveChanges();
                        }
                    }

                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                
                
            }
        }
    }
}
