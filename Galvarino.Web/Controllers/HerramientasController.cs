using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Controllers
{
    public class HerramientasController : Controller
    {
        
        private readonly IConfiguration _configuration;

        public HerramientasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public IActionResult Index()
        {
            return Ok();
            
           
        }
        
    }
}
