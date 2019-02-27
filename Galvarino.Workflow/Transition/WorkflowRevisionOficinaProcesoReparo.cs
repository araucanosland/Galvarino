using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowRevisionOficinaProcesoReparo : WorkflowTransitionValidation
    {
        public WorkflowRevisionOficinaProcesoReparo(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            if (Variable("REPARO_OFICINA_PROCESO") == "1")
            {
                return true;
            }
            return false;
        }
    }
}
