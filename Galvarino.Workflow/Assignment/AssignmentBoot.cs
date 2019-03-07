using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Galvarino.Workflow.Infrastructure;
using Galvarino.Workflow.Model;

namespace Galvarino.Workflow.Assignment
{
    public class AssignmentBoot : IAssignor
    {
        protected readonly WorkflowDbContext _context;
        protected Dictionary<string, Variable> _variables;
        public AssignmentBoot(WorkflowDbContext context, string numeroTicket)
        {
            _context = context;
            _variables = _context.Variables.Where(v => v.NumeroTicket == numeroTicket).ToDictionary(x => x.Clave);
        }
    }
}
