using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowExpedienteCompleto : WorkflowTransitionValidation
    {
        public WorkflowExpedienteCompleto(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Convert.ToInt32(Variable("ES_RM")) == 0 && Convert.ToInt32(Variable("EXPEDIENTE_FALTANTE")) == 0)
            {
                ret = true;
            }
            return ret;
        }
    }
}
