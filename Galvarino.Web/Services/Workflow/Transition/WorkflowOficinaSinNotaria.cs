using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowOficinaSinNotaria : WorkflowTransitionValidation
    {
        public WorkflowOficinaSinNotaria(ApplicationDbContext context, string numeroTicket) 
                :base(context, numeroTicket)
        {
        }
        
        public override bool Validar()
        {
            bool retorno = false;

            if (Variable("ES_RM").Equals("0") && Variable("OFINA_PROCESA_NOTARIA") != Variable("OFICINA_PAGO"))
            {
                retorno = true;
            }

            return retorno;
        }
    }
}
