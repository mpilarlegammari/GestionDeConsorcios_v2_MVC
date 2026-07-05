using GestionDeConsorcios_v2_MVC.Context;
using GestionDeConsorcios_v2_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionDeConsorcios_v2_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public HomeController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

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
            var cantidadConsorcios = _context.Consorcios.Count();
            var cantidadGastos = _context.Gastos.Count();

            var expensasPendientes = _context.Expensas
                .Count(e => e.Estado == EstadoExpensa.Pendiente);

            var pagosPendientesRevision = _context.Pagos
                .Count(p => p.Estado == EstadoPago.PendienteRevision);

            var reclamosAbiertos = _context.Reclamos
                .Count(r => r.Estado == EstadoReclamo.Abierto);

            var reservasPendientes = _context.Reservas
                .Count(r => r.Estado == EstadoReserva.Pendiente);

            ViewBag.CantidadConsorcios = cantidadConsorcios;
            ViewBag.CantidadGastos = cantidadGastos;
            ViewBag.ExpensasPendientes = expensasPendientes;
            ViewBag.PagosPendientesRevision = pagosPendientesRevision;
            ViewBag.ReclamosAbiertos = reclamosAbiertos;
            ViewBag.ReservasPendientes = reservasPendientes;

            ViewBag.AsuntosPendientes =
                expensasPendientes +
                pagosPendientesRevision +
                reclamosAbiertos +
                reservasPendientes;

            return View();
        }

        public IActionResult PropietarioHome()
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