using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ExpensasController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public ExpensasController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: EXPENSAS
    public async Task<IActionResult> Index()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
            return RedirectToAction("Login", "Auth");

        if (rol == "Administrador")
        {
            var expensas = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                .ToListAsync();

            return View(expensas);
        }

        if (rol == "Propietario")
        {
            int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
            if (unidadFuncionalId == null)
                return RedirectToAction("Login", "Auth");

            var expensas = await _context.Expensas
                .Where(e => e.UnidadFuncionalId == unidadFuncionalId.Value)
                .Include(e => e.UnidadFuncional)
                .ToListAsync();

            return View(expensas);
        }

        return RedirectToAction("Login", "Auth");
    }

    // GET: EXPENSAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (string.IsNullOrEmpty(rol))
            return RedirectToAction("Login", "Auth");

        Expensa? expensa = null;

        if (rol == "Administrador")
        {
            expensa = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                .FirstOrDefaultAsync(m => m.Id == id.Value);
        }
        else if (rol == "Propietario")
        {
            int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
            if (unidadFuncionalId == null)
                return RedirectToAction("Login", "Auth");

            expensa = await _context.Expensas
                .Where(e => e.Id == id.Value && e.UnidadFuncionalId == unidadFuncionalId.Value)
                .Include(e => e.UnidadFuncional)
                .FirstOrDefaultAsync();
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }

        if (expensa == null)
            return NotFound();

        return View(expensa);
    }

    // GET: EXPENSAS/Create
    public IActionResult Create()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (string.IsNullOrEmpty(rol) || rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        ViewBag.Unidades = new SelectList(_context.UnidadesFuncionales, "Id", "NumeroUF");
        return View();
    }

    // POST: EXPENSAS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UnidadFuncionalId,Periodo,FechaEmision,FechaVencimiento,MontoTotal,Estado,Observaciones")] Expensa expensa)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");
        if (string.IsNullOrEmpty(rol) || rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        ModelState.Remove("UnidadFuncional");

        if (ModelState.IsValid)
        {
            _context.Expensas.Add(expensa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Unidades = new SelectList(_context.UnidadesFuncionales, "Id", "NumeroUF", expensa.UnidadFuncionalId);
        return View(expensa);
    }

    // GET: EXPENSAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol) || rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        if (id == null)
            return NotFound();

        var expensa = await _context.Expensas.FindAsync(id);

        if (expensa == null)
            return NotFound();

        ViewBag.Unidades = new SelectList(_context.UnidadesFuncionales, "Id", "NumeroUF", expensa.UnidadFuncionalId);

        return View(expensa);
    }

    // POST: EXPENSAS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,UnidadFuncionalId,Periodo,FechaEmision,FechaVencimiento,MontoTotal,Estado,Observaciones")] Expensa expensa)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol) || rol != "Administrador")
            return RedirectToAction("Login", "Auth");

        if (id != expensa.Id)
            return NotFound();

        ModelState.Remove("UnidadFuncional");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(expensa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpensaExists(expensa.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Unidades = new SelectList(_context.UnidadesFuncionales, "Id", "NumeroUF", expensa.UnidadFuncionalId);
        return View(expensa);
    }

    private bool ExpensaExists(int? id)
    {
        return _context.Expensas.Any(e => e.Id == id);
    }
}
