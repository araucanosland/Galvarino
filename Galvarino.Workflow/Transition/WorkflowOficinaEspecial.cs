using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;

namespace Galvarino.Workflow.Transition
{
    public class WorkflowOficinaEspecial : WorkflowTransitionValidation
    {
        public WorkflowOficinaEspecial(WorkflowDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;

            if (Variable("ES_RM").Equals("0") && Variable("OFICINA_PROCESA_NOTARIA") != Variable("OFICINA_PAGO"))
            {
                retorno = true;
            }

            return retorno;
        }
    }
}
