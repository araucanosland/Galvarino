using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowCreditoRM : WorkflowTransitionValidation
    {
        public WorkflowCreditoRM(WorkflowDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;
            try{
                if (Convert.ToInt32(Variable("ES_RM")) == 1)
                {
                    retorno = true;
                }
            }
            catch(Exception)
            {
                retorno = false;
            }
            return retorno;
        }
    }
}
