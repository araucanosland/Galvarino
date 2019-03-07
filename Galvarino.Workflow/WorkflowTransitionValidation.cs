using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Infrastructure;
using Galvarino.Workflow.Model;

namespace Galvarino.Workflow
{
    public class WorkflowTransitionValidation : IWorkflowTransitionValidation
    {

        
        protected readonly WorkflowDbContext _context;

        public WorkflowTransitionValidation(WorkflowDbContext context, string numeroTicket)
        {
            _context = context;
            Variables = _context.Variables.Where(v => v.NumeroTicket.Equals(numeroTicket));
            NumeroTicket = numeroTicket;
        }

        protected string NumeroTicket { get; set; }

        protected IEnumerable<Variable> Variables { get; }

        protected string Variable(string clave)
        {
            var varu = Variables.FirstOrDefault(v => v.Clave.Equals(clave));
            return varu != null ? varu.Valor : "";
            
        }

        public virtual bool Validar()
        {
            return true;
        }
    }
}
