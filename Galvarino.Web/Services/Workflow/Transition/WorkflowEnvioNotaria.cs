using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowEnvioNotaria : WorkflowTransitionValidation
    {
        public WorkflowEnvioNotaria(ApplicationDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = true;
            if (Variable("LEGALIZADO_ANTES") == "1" || Variable("ACUERDO_PAGO_TOTAL") == "1" || Variable("NULO") == "1")  
            {
                retorno = false;
            }
            return retorno;
        }
    }
}
