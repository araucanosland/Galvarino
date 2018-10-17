using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowExpedienteCompleto : WorkflowTransitionValidation
    {
        public WorkflowExpedienteCompleto(ApplicationDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Convert.ToInt32(Variable("EXPEDIENTE_FALTANTE")) == 0)
            {
                ret = true;
            }
            return ret;
        }
    }
}
