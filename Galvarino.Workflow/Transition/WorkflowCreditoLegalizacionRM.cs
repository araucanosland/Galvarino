using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowCreditoLegalizacionRM : WorkflowTransitionValidation
    {
        public WorkflowCreditoLegalizacionRM(WorkflowDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;
            try
            {
                if (Convert.ToInt32(Variable("ES_RM")) == 1 && Convert.ToInt32(Variable("EXPEDIENTE_FALTANTE")) == 0)
                {
                    retorno = true;
                }
            }
            catch (Exception)
            {
                retorno = false;
            }
            return retorno;
        }
    }
}
