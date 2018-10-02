using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        public AuthController()
        {
            
        }
        [HttpPost]
        [Route("challenge")]
        public IActionResult Autehticate()
        {
            return Ok();
        }

        
    }
}
