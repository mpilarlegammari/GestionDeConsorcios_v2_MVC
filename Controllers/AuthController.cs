using GestionDeConsorcios_v2_MVC.Models;
using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionDeConsorcios_v2_MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public AuthController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new Usuario());
        }

        [HttpPost]
        public IActionResult Login(Usuario usuario)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == usuario.Email && u.PasswordHash == usuario.PasswordHash && u.Activo);
            if (user != null)
            {
                if (user.Rol == RolUsuario.Administrador)
                {
                    // Redirigir a la vista de administrador
                    return RedirectToAction("AdminHome", "Home");
                }
                else if (user.Rol == RolUsuario.Propietario)
                {
                    // Redirigir a la vista de propietario
                    return RedirectToAction("PropietarioHome", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
            return View(usuario);
        }

        public IActionResult Index()
        {
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
