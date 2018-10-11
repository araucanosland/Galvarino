using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowRecepcionNotariaOk : WorkflowTransitionValidation
    {
        public WorkflowRecepcionNotariaOk(ApplicationDbContext context, string numeroTicket)
                : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool ret = false;
            if (Convert.ToInt32(Variable("REPARO_REVISION_DOCUMENTO_LEGALIZADO")) == 0)
            {
                ret = true;
            }
            return ret;
        }
    }
}
