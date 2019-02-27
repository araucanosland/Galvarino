using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowCreditoLegalizado : WorkflowTransitionValidation
    {
        public WorkflowCreditoLegalizado(WorkflowDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;

            if (Variable("DOCUMENTO_LEGALIZADO").Equals("1"))
            {
                retorno = true;
            }

            return retorno;
        }
    }
}
