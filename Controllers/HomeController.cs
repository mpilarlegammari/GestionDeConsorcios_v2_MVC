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

            var uf = await _context.UnidadesFuncionales
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == ufId.Value);

            if (uf == null)
                return RedirectToAction("Login", "Auth");

            var hoy = DateTime.Today;

            decimal totalExpensas = await _context.Expensas
                .Where(e => e.UnidadFuncionalId == ufId.Value && e.FechaEmision < hoy)
                .SumAsync(e => (decimal?)e.MontoTotal) ?? 0;

            decimal totalPagos = await _context.Pagos
                .Where(p =>
                    p.Expensa.UnidadFuncionalId == ufId.Value &&
                    p.Estado == EstadoPago.Aprobado)
                .SumAsync(p => (decimal?)p.MontoPagado) ?? 0;

            decimal saldo = totalExpensas - totalPagos;

            var proximaExpensa = await _context.Expensas
                .Where(e =>
                    e.UnidadFuncionalId == ufId.Value &&
                    e.FechaVencimiento >= hoy &&
                    e.Estado != EstadoExpensa.Pagada)
                .OrderBy(e => e.FechaVencimiento)
                .FirstOrDefaultAsync();

            int pagosPendientesRevision = await _context.Pagos
                .CountAsync(p =>
                    p.UnidadFuncionalId == ufId.Value &&
                    p.Estado == EstadoPago.PendienteRevision);

            var proximaReserva = await _context.Reservas
                .Include(r => r.Amenity)
                .Where(r =>
                    r.UnidadFuncionalId == ufId.Value &&
                    r.FechaReserva >= hoy &&
                    r.Estado != EstadoReserva.Cancelada &&
                    r.Estado != EstadoReserva.Rechazada)
                .OrderBy(r => r.FechaReserva)
                .FirstOrDefaultAsync();

            int reclamosActivos = await _context.Reclamos
                .CountAsync(r =>
                    r.UnidadFuncionalId == ufId.Value &&
                    r.Estado != EstadoReclamo.Cerrado);

            var ultimosComunicados = await _context.Comunicados
                .Where(c => c.ConsorcioId == uf.ConsorcioId)
                .OrderByDescending(c => c.Importante)
                .ThenByDescending(c => c.FechaPublicacion)
                .Take(2)
                .ToListAsync();

            ViewBag.TotalExpensas = totalExpensas;
            ViewBag.TotalPagos = totalPagos;
            ViewBag.Saldo = saldo;
            ViewBag.TieneDeuda = saldo > 0;

            ViewBag.ProximaExpensa = proximaExpensa;
            ViewBag.PagosPendientesRevision = pagosPendientesRevision;
            ViewBag.ProximaReserva = proximaReserva;
            ViewBag.ReclamosActivos = reclamosActivos;
            ViewBag.UltimosComunicados = ultimosComunicados;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}