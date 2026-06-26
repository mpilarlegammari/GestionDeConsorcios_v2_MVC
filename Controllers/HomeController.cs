using GestionDeConsorcios_v2_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionDeConsorcios_v2_MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AdminHome()
        {
            // Redirige a la vista de administrador
            return View();
        }

        public IActionResult PropietarioHome()
        {
            // Redirige a la vista de propietario
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
