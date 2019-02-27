using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowApproved : WorkflowTransitionValidation
    {
        public WorkflowApproved(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            return true;
        }
    }
}
