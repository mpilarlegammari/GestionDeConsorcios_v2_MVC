using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

    // GET: EXPENSAS/Generar
    public IActionResult Generar()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre");

        ViewBag.Periodo = DateTime.Today.ToString("yyyy-MM");
        ViewBag.FechaEmision = DateTime.Today.ToString("yyyy-MM-dd");
        ViewBag.FechaVencimiento = DateTime.Today.AddDays(10).ToString("yyyy-MM-dd");

        return View();
    }

    // POST: EXPENSAS/Generar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(
        int consorcioId,
        string periodo,
        DateTime fechaEmision,
        DateTime fechaVencimiento,
        string? observaciones)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        periodo = periodo?.Trim() ?? string.Empty;

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", consorcioId);
        ViewBag.Periodo = periodo;
        ViewBag.FechaEmision = fechaEmision.ToString("yyyy-MM-dd");
        ViewBag.FechaVencimiento = fechaVencimiento.ToString("yyyy-MM-dd");
        ViewBag.Observaciones = observaciones;

        if (consorcioId <= 0)
        {
            ModelState.AddModelError("consorcioId", "Debe seleccionar un consorcio.");
        }

        if (string.IsNullOrWhiteSpace(periodo))
        {
            ModelState.AddModelError("periodo", "Debe ingresar un período.");
        }

        if (!DateTime.TryParseExact(periodo, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaPeriodo))
        {
            ModelState.AddModelError("periodo", "El período debe tener el formato AAAA-MM. Ejemplo: 2026-03.");
        }

        if (fechaVencimiento <= fechaEmision)
        {
            ModelState.AddModelError("fechaVencimiento", "La fecha de vencimiento debe ser posterior a la fecha de emisión.");
        }

        var consorcioExiste = await _context.Consorcios.AnyAsync(c => c.Id == consorcioId);

        if (!consorcioExiste)
        {
            ModelState.AddModelError("consorcioId", "El consorcio seleccionado no existe.");
        }

        var unidades = await _context.UnidadesFuncionales
            .Where(u => u.ConsorcioId == consorcioId)
            .ToListAsync();

        if (unidades.Count == 0)
        {
            ModelState.AddModelError("consorcioId", "El consorcio no tiene unidades funcionales cargadas.");
        }

        var yaExistenExpensas = await _context.Expensas
            .Include(e => e.UnidadFuncional)
            .AnyAsync(e =>
                e.Periodo == periodo &&
                e.UnidadFuncional.ConsorcioId == consorcioId);

        if (yaExistenExpensas)
        {
            ModelState.AddModelError("periodo", "Ya existen expensas generadas para ese consorcio y período.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        var inicioMes = new DateTime(fechaPeriodo.Year, fechaPeriodo.Month, 1);
        var finMes = inicioMes.AddMonths(1);

        var gastosDelPeriodo = await _context.Gastos
            .Where(g =>
                g.ConsorcioId == consorcioId &&
                g.Fecha >= inicioMes &&
                g.Fecha < finMes)
            .ToListAsync();

        if (gastosDelPeriodo.Count == 0)
        {
            ModelState.AddModelError("periodo", "No hay gastos cargados para ese consorcio y período.");
            return View();
        }

        var totalGastos = gastosDelPeriodo.Sum(g => g.Monto);
        var montoPorUnidad = totalGastos / unidades.Count;

        foreach (var unidad in unidades)
        {
            var expensa = new Expensa
            {
                UnidadFuncionalId = unidad.Id,
                Periodo = periodo,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                MontoTotal = montoPorUnidad,
                Estado = EstadoExpensa.Pendiente,
                Observaciones = observaciones
            };

            _context.Expensas.Add(expensa);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Se generaron {unidades.Count} expensas para el período {periodo}.";

        return RedirectToAction(nameof(Index));
    }
    /*
    //Vieja version para crear expensas manualmente. Se necesita volver a crear el archivo Create.cshtml para que funcione.
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
    }*/

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
