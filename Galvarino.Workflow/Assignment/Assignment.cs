using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Workflow.Assignment
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Assignment : Attribute
    {
        public string AssginName { get; set; }
    }
}
