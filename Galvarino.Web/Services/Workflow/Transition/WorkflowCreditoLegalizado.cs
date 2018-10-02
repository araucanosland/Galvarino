using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowCreditoLegalizado : WorkflowTransitionValidation
    {
        public WorkflowCreditoLegalizado(ApplicationDbContext context, string numeroTicket) : base(context, numeroTicket)
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
