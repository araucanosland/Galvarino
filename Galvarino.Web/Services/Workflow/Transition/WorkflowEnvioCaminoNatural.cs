﻿using System;
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
            bool retorno = true;
            try
            {
                if (Variable("LEGALIZADO_ANTES") == "1" || Variable("ACUERDO_PAGO_TOTAL") == "1" || Variable("NULO") == "1")
                {
                    retorno = false;
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
