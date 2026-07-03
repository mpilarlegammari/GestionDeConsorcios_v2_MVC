using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class GastosController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public GastosController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: GASTOS
    public async Task<IActionResult> Index()    
    {

        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol == RolUsuario.Administrador.ToString())
        {        // Incluir el consorcio 
            var gastos = await _context.Gastos
                .Include(g => g.Consorcio)
                .ToListAsync();

            return View("AdminIndex", gastos);
        }

        if (rol == RolUsuario.Propietario.ToString())
        {
            var unidadFuncionalId =
                HttpContext.Session.GetInt32("UnidadFuncionalId");

            if (unidadFuncionalId == null)
                return RedirectToAction("Login", "Auth");

            var consorcioId = await _context.UnidadesFuncionales
                .Where(u => u.Id == unidadFuncionalId)
                .Select(u => u.ConsorcioId)
                .FirstOrDefaultAsync();

            var gastos = await _context.Gastos
                .Include(g => g.Consorcio)
                .Where(g => g.ConsorcioId == consorcioId)
                .ToListAsync();

            return View("PropietarioIndex", gastos);
        }

        return RedirectToAction("Login", "Auth");



    }

    // GET: GASTOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gasto = await _context.Gastos
            .Include(g => g.Consorcio)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (gasto == null)
        {
            return NotFound();
        }

        return View(gasto);
    }

    // GET: GASTOS/Create
    public IActionResult Create()
    {
        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre");
        return View();
    }

    // POST: GASTOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gasto gasto, IFormFile? archivoFactura)
    {
        if (ModelState.IsValid)
        {
            if (archivoFactura != null && archivoFactura.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "facturas");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoFactura.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivoFactura.CopyToAsync(stream);
                }
                gasto.ArchivoFacturaPath = "/facturas/" + uniqueFileName;
            }
            gasto.FechaCreacion = DateTime.UtcNow;
            _context.Add(gasto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre");
        return View(gasto);
    }

    // GET: GASTOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null)
        {
            return NotFound();
        }
        // Poblar lista de consorcios para el dropdown (mostrar Nombre pero almacenar Id)
        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", gasto.ConsorcioId);
        return View(gasto);
    }

    // POST: GASTOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ConsorcioId,NumeroFactura,Fecha,Monto,Concepto,Categoria,ArchivoFacturaPath,Descripcion")] Gasto gasto)
    {
        if (id != gasto.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(gasto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GastoExists(gasto.Id))
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
        // Si hay errores, repoblar la lista para que el dropdown se muestre correctamente
        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", gasto.ConsorcioId);
        return View(gasto);
    }

    // GET: GASTOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gasto = await _context.Gastos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (gasto == null)
        {
            return NotFound();
        }

        return View(gasto);
    }

    // POST: GASTOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto != null)
        {
            _context.Gastos.Remove(gasto);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool GastoExists(int? id)
    {
        return _context.Gastos.Any(e => e.Id == id);
    }
}
