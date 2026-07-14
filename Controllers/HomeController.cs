using GestionDeConsorcios_v2_MVC.Context;
using GestionDeConsorcios_v2_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> PropietarioHome()
        {
            int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");

            if (ufId == null)
                return RedirectToAction("Login", "Auth");

            decimal totalExpensas = await _context.Expensas
                .Where(e => e.UnidadFuncionalId == ufId.Value && e.FechaVencimiento<DateTime.Today)
                .SumAsync(e => (decimal?)e.MontoTotal) ?? 0;

            decimal totalPagos = await _context.Pagos
                .Where(p =>
                    p.Expensa.UnidadFuncionalId == ufId.Value &&
                    p.Estado == EstadoPago.Aprobado)
                .SumAsync(p => (decimal?)p.MontoPagado) ?? 0;

            decimal saldo = totalExpensas - totalPagos;
            bool tieneDeuda = saldo > 0;

            ViewBag.TotalExpensas = totalExpensas;
            ViewBag.TotalPagos = totalPagos;
            ViewBag.Saldo = saldo;
            ViewBag.TieneDeuda = tieneDeuda;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}