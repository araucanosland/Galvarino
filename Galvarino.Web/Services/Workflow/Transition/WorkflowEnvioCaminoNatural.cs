using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;

namespace Galvarino.Web.Services.Workflow.Transition
{
    public class WorkflowEnvioCaminoNatural : WorkflowTransitionValidation
    {
        public WorkflowEnvioCaminoNatural(ApplicationDbContext context, string numeroTicket) : base(context, numeroTicket)
        {
        }

        public override bool Validar()
        {
            bool retorno = false;
            try
            {
                if (Variable("LEGALIZADO_ANTES") == "0" || Variable("ACUERDO_PAGO_TOTAL") == "0" || Variable("NULO") == "0")
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
