using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Workflow;

namespace Galvarino.Web.Services.Workflow.Assignment
{
    public class AssignmentBoot : IAssignor
    {
        protected readonly ApplicationDbContext _context;
        protected Dictionary<string, Variable> _variables;
        public AssignmentBoot(ApplicationDbContext context, string numeroTicket)
        {
            _context = context;
            _variables = _context.Variables.Where(v => v.NumeroTicket == numeroTicket).ToDictionary(x => x.Clave);
        }
    }
}
