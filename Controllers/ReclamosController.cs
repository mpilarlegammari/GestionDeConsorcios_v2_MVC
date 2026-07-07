
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;

public class ReclamosController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public ReclamosController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: RECLAMOS
    public async Task<IActionResult> Index()
    { 

        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (rol == "Administrador")
        {
            var reclamosAdmin = await _context.Reclamos.Include(r => r.UnidadFuncional).ToListAsync();
            return View(reclamosAdmin);
        }

        if (rol == "Propietario")
        {
            int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
            if (unidadFuncionalId == null)
                return RedirectToAction("Login", "Auth");

            var reclamosPropietario = await _context.Reclamos.Include(r => r.UnidadFuncional).Where(r => r.UnidadFuncionalId == unidadFuncionalId.Value).ToListAsync();

            return View(reclamosPropietario);
        }

        // Rol desconocido: redirigir a login por seguridad
        return RedirectToAction("Login", "Auth");


    }












    // GET: RECLAMOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reclamo = await _context.Reclamos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reclamo == null)
        {
            return NotFound();
        }

        return View(reclamo);
    }

    // GET: RECLAMOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: RECLAMOS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,UnidadFuncionalId,Asunto,Categoria,Descripcion,Estado,FechaCreacion,FechaCierre,ObservacionAdministracion,UnidadFuncional")] Reclamo reclamo)
    {
        if (ModelState.IsValid)
        {
            _context.Add(reclamo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(reclamo);
    }

    // GET: RECLAMOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id == null)
        {
            return NotFound();
        }

        var reclamo = await _context.Reclamos
            .Include(r => r.UnidadFuncional)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reclamo == null)
        {
            return NotFound();
        }

        return View(reclamo);
    }

    // POST: RECLAMOS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Estado,FechaCierre,ObservacionAdministracion")] Reclamo datos)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        var reclamo = await _context.Reclamos.FindAsync(id);

        if (reclamo == null)
        {
            return NotFound();
        }

        if (id != datos.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            reclamo.Estado = datos.Estado;
            reclamo.FechaCierre = datos.FechaCierre;
            reclamo.ObservacionAdministracion = datos.ObservacionAdministracion;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(reclamo);
    }

    // GET: RECLAMOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reclamo = await _context.Reclamos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reclamo == null)
        {
            return NotFound();
        }

        return View(reclamo);
    }

    // POST: RECLAMOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var reclamo = await _context.Reclamos.FindAsync(id);
        if (reclamo != null)
        {
            _context.Reclamos.Remove(reclamo);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ReclamoExists(int? id)
    {
        return _context.Reclamos.Any(e => e.Id == id);
    }
}
