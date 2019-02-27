using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowRejected : WorkflowTransitionValidation
    {
        public WorkflowRejected(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            return false;
        }
    }
}
