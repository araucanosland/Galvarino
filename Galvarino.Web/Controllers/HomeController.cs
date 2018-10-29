using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;

namespace Galvarino.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;

        public HomeController(SignInManager<Usuario> signInManager)
        {
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                return Redirect("/wf/v1/mis-solicitudes");
            }
            return View();
        }

        public IActionResult SignOut()
        {
            _signInManager.SignOutAsync();
            return Redirect("/");
        }

        public IActionResult SinPermiso()
        {
            return View();
        }
        
        public IActionResult NuevoUsuario()
        {
            return View();
        }

        public IActionResult Principal()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
