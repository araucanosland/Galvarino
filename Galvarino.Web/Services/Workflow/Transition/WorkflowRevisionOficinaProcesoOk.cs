using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowRevisionOficinaProcesoOk : WorkflowTransitionValidation
    {
        public WorkflowRevisionOficinaProcesoOk(ApplicationDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if(Variables.Where(x=>x.Clave == "REPARO_OFICINA_PROCESO").ToList().Count > 0)
            {
                if (Variable("REPARO_OFICINA_PROCESO") =="" || Variable("REPARO_OFICINA_PROCESO") == "0")
                {
                    ret = true;
                }
            }
            else
            {
                ret = true;
            }
            
            
            return ret;
        }
    }
}
