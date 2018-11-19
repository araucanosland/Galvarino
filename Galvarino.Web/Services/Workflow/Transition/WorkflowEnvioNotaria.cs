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
            bool retorno = false;
            try
            {
                if (Convert.ToInt32(Variable("LEGALIZADO_ANTES")) == 0)
                {
                    retorno = true;
                }
            }
            catch (Exception)
            {
                //Si no existe variable por defectgo avanza
                retorno = true;
            }
            return retorno;
        }
    }
}
