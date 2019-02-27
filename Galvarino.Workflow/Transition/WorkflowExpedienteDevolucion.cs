using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowExpedienteDevolucion : WorkflowTransitionValidation
    {
        public WorkflowExpedienteDevolucion(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Convert.ToInt32(Variable("DEVOLUCION_A_SUCURSAL")) == 1)
            {
                ret = true;
            }
            return ret;
        }
    }
}