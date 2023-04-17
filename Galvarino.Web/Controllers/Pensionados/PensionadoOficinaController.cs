using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Services.Workflow;
using System.Security.Claims;
using System.IO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Galvarino.Web.Controllers.Pensionados
{
    
    public class PensionadoOficinaController : Controller
    {
        public IActionResult PrepararNominaDespacho()
        {
   
            var texto = "pagina Pensionados";
            ViewBag.editando = texto;
            return View();
        }
    }
}
