
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
            .ToListAsync();
        return View(reservas);

    }
    public async Task<IActionResult> IndexPropietario()
    {
        int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
        if (unidadFuncionalId == null)
            return RedirectToAction("Login", "Auth");

        var reservas = await _context.Reservas.Include(r => r.UnidadFuncional).Where(r => r.UnidadFuncionalId == unidadFuncionalId.Value).Include(r => r.Amenity).ToListAsync();

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
        if (ModelState.IsValid)
        {
            int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");

            if (ufId == null)
                return RedirectToAction("Login", "Auth");

            reserva.UnidadFuncionalId = (int)ufId;

            _context.Add(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
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
}
