using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowSc : WorkflowTransitionValidation
    {
        public WorkflowSc(ApplicationDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;
            try
            {

                var elexpediente = _context.ExpedientesComplementarios.Where(x => x.NumeroTicket == NumeroTicket).FirstOrDefault();

                var elDocumento = _context.Documentos.Include(t => t.ExpedienteCredito).ThenInclude(d => d.Credito).Where(x => x.ExpedienteCredito.Credito.Id == elexpediente.CreditoId).FirstOrDefault();


                   // _context.Tareas.Include(t => t.Solicitud).ThenInclude(x => x.Proceso).Where(t => t.AsignadoA == identificacionUsuario && t.Solicitud.Proceso.NombreInterno == nombreInternoProceso && t.Estado == EstadoTarea.Activada && t.FechaTerminoFinal == null).ToList();




                if (Convert.ToInt32(Variable("ES_RM")) == 1 && Convert.ToInt32(Variable("EXPEDIENTE_FALTANTE")) == 0)
                {
                    retorno = true;
                }
            }
            catch (Exception)
            {
                retorno = false;
            }
            return retorno;
        }

    }
}
