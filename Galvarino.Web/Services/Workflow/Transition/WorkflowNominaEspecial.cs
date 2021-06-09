using Galvarino.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowNominaEspecial : WorkflowTransitionValidation
    {
        public WorkflowNominaEspecial(ApplicationDbContext context, string numeroTicket)
               : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Variable("NULO") == "1" || Variable("ACUERDO_PAGO_TOTAL") == "1")
            {
                ret = true;
            }
            return ret;
        }

    }
}
