using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowRecepcionNotariaReparo : WorkflowTransitionValidation
    {
        public WorkflowRecepcionNotariaReparo(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Convert.ToInt32(Variable("REPARO_REVISION_DOCUMENTO_LEGALIZADO")) > 0)
            {
                ret = true;
            }
            return ret;
        }
    }
}
