
using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ReservasController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public ReservasController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: RESERVAS
    public async Task<IActionResult> Index()    
    {

        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (rol == "Administrador")
        {

            return RedirectToAction("IndexAdmin", "Reservas");
        }

        if (rol == "Propietario")
        {
            return RedirectToAction("IndexPropietario", "Reservas");
        }

        // Rol desconocido: redirigir a login por seguridad
        return RedirectToAction("Login", "Auth");

    }



    public async Task<IActionResult> IndexAdmin()
    {
        // incluyo la UF y su Consorcio para mostrar NumeroUF y Nombre del Consorcio en la vista
        var reservas = await _context.Reservas
            .Include(r => r.UnidadFuncional)
                .ThenInclude(uf => uf.Consorcio)
                .Include(r => r.Amenity)
                .OrderDescending()
            .ToListAsync();
        return View(reservas);

    }
    public async Task<IActionResult> IndexPropietario()
    {
        int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
        if (unidadFuncionalId == null)
            return RedirectToAction("Login", "Auth");

        var reservas = await _context.Reservas
            .Include(r => r.UnidadFuncional)
            .Where(r => r.UnidadFuncionalId == unidadFuncionalId.Value)
            .Include(r => r.Amenity)
            .OrderDescending()
            .ToListAsync();

        return View(reservas);

    }

    // GET: RESERVAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reserva == null)
        {
            return NotFound();
        }

        return View(reserva);
    }

    // GET: RESERVAS/Create
    public async Task<IActionResult> Create()
    {
        int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
        if (unidadFuncionalId == null)
            return RedirectToAction("Login", "Auth");

        var uf = _context.UnidadesFuncionales.FirstOrDefault(uf => uf.Id == unidadFuncionalId.Value);

        var amenities = await _context.Amenities
            .Include(a => a.Consorcio)
                .Where(a => a.ConsorcioId == uf.ConsorcioId)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        ViewBag.Amenities = new SelectList(amenities, "Id", "Nombre");

        return View();


    }

    // POST: RESERVAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AmenityId,FechaReserva,Observaciones,Turno")] Reserva reserva)
    {
        int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");

        if (ufId == null)
            return RedirectToAction("Login", "Auth");

        var uf = await _context.UnidadesFuncionales
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == ufId.Value);

        if (uf == null)
            return RedirectToAction("Login", "Auth");

        if (ModelState.IsValid && await TieneDeudaAsync(ufId.Value))
        {
            ModelState.AddModelError(
                string.Empty,
                "No puede realizar una reserva porque registra expensas pendientes de pago.");
        }

        if (ModelState.IsValid && await ExisteReservaEnMismoTurnoAsync(reserva, uf.ConsorcioId))
        {
            ModelState.AddModelError(
                nameof(reserva.Turno),
                "Ya existe una reserva para ese espacio, fecha y turno.");
        }

        if (ModelState.IsValid)
        {
            reserva.UnidadFuncionalId = ufId.Value;

            _context.Add(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        var amenities = await _context.Amenities
            .Where(a => a.ConsorcioId == uf.ConsorcioId)
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        ViewBag.Amenities = new SelectList(
            amenities,
            "Id",
            "Nombre",
            reserva.AmenityId);

        return View(reserva);
    }

    // GET: RESERVAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound();
        }
        return View(reserva);
    }

    // POST: RESERVAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,AmenityId,UnidadFuncionalId,FechaReserva,Estado,Observaciones,Turno,Amenity,UnidadFuncional")] Reserva reserva)
    {
        if (id != reserva.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(reserva);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservaExists(reserva.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(reserva);
    }

    // GET: RESERVAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reserva == null)
        {
            return NotFound();
        }

        return View(reserva);
    }

    // POST: RESERVAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva != null)
        {
            _context.Reservas.Remove(reserva);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ReservaExists(int? id)
    {
        return _context.Reservas.Any(e => e.Id == id);
    }

    private Task<bool> ExisteReservaEnMismoTurnoAsync(Reserva reserva, int consorcioId)
    {
        return _context.Reservas.AnyAsync(r =>
            r.AmenityId == reserva.AmenityId &&
            r.Amenity.ConsorcioId == consorcioId &&
            r.FechaReserva.Date == reserva.FechaReserva.Date &&
            r.Turno == reserva.Turno &&
            r.Estado != EstadoReserva.Cancelada);
    }

    private async Task<bool> TieneDeudaAsync(int unidadFuncionalId)
    {
        decimal totalExpensas = await _context.Expensas
            .Where(e => e.UnidadFuncionalId == unidadFuncionalId && e.FechaVencimiento<DateTime.Today)
            .SumAsync(e => (decimal?)e.MontoTotal) ?? 0;

        decimal totalPagos = await _context.Pagos
            .Where(p =>
                p.Expensa.UnidadFuncionalId == unidadFuncionalId &&
                p.Estado == EstadoPago.Aprobado)
            .SumAsync(p => (decimal?)p.MontoPagado) ?? 0;

        return totalPagos < totalExpensas;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva == null)
            return NotFound();

        if (reserva.Estado == EstadoReserva.Cancelada)
        {
            TempData["Error"] = "No se puede aprobar una reserva cancelada.";
            return RedirectToAction(nameof(IndexAdmin));
        }

        if (reserva.Estado == EstadoReserva.Confirmada)
        {
            TempData["Error"] = "La reserva ya se encuentra aprobada.";
            return RedirectToAction(nameof(IndexAdmin));
        }

        reserva.Estado = EstadoReserva.Confirmada;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La reserva fue aprobada correctamente.";

        return RedirectToAction(nameof(IndexAdmin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva == null)
            return NotFound();

        if (reserva.Estado == EstadoReserva.Cancelada)
        {
            TempData["Error"] = "No se puede aprobar una reserva cancelada.";
            return RedirectToAction(nameof(IndexAdmin));
        }

        reserva.Estado = EstadoReserva.Rechazada;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La reserva fue rechazada correctamente.";

        return RedirectToAction(nameof(IndexAdmin));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Propietario")
            return RedirectToAction("Login", "Auth");

        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva == null)
            return NotFound();

        reserva.Estado = EstadoReserva.Cancelada;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La reserva fue cancelada correctamente.";

        return RedirectToAction(nameof(IndexPropietario));
    }
}
