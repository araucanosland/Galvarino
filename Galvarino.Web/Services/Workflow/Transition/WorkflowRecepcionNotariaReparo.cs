using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowRecepcionNotariaReparo : WorkflowTransitionValidation
    {
        public WorkflowRecepcionNotariaReparo(ApplicationDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Variable("REPARO_REPECION_NOTARIA") == "1")
            {
                ret = true;
            }
            return ret;
        }
    }
}
