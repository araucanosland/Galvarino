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
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Galvarino.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;

        public HomeController(SignInManager<Usuario> signInManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _configuration = configuration;
        }
        
        public IActionResult Index()

        {
            string userAgent = Request.Headers["User-Agent"].ToString();
            if(userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
            {
                return Content("Internet Explorer no soporta esta App.");
            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    return Redirect("/wf/v1/mis-solicitudes");
                }
                else
                {
                    string route = _configuration["RutaAutenticacionUsuario"] + "?code=" + _configuration["CodigoSistema"];
                    return Redirect(route);
                }
            }
            
           
        }

        public async Task<IActionResult> SignIn(string rut)
        {        
            try
            {
                var user = await _signInManager.UserManager.FindByNameAsync(rut);
                await _signInManager.SignInAsync(user, true);
                //if(User.Identity.IsAuthenticated)
                //{
                return Redirect("/wf/v1/mis-solicitudes");
                //}
                //else
                //{
                //    return View("SinPermiso");
                //}
                
            }
            catch (Exception ex)
            {
                return View("SinPermiso");
            }
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
