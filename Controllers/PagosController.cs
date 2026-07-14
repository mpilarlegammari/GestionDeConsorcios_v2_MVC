using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;

public class PagosController : Controller
{
    private readonly GestionDeConsorciosContext _context;
    private readonly IWebHostEnvironment _environment;

    public PagosController(
        GestionDeConsorciosContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: PAGOS
    public async Task<IActionResult> Index()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (rol == "Administrador")
        {
            return RedirectToAction("IndexAdmin", "Pagos");
        }

        if (rol == "Propietario")
        {
            
                return RedirectToAction("IndexPropietario", "Pagos");
        }

        return RedirectToAction("Login", "Auth");
    }

    public async Task<IActionResult> IndexAdmin()
    {
        var pagosAdmin = await _context.Pagos
        .Include(p => p.Expensa)
        .Include(p => p.UnidadFuncional)
                        .ThenInclude(uf => uf.Consorcio)
                        .OrderDescending()
        .ToListAsync();

        return View(pagosAdmin);
    }

    public async Task<IActionResult> IndexPropietario()
    {
        int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");
        if (unidadFuncionalId == null)
            return RedirectToAction("Login", "Auth");

        var pagosPropietario = await _context.Pagos
            .Where(p => p.UnidadFuncionalId == unidadFuncionalId.Value)
            .Include(p => p.Expensa)
            .Include(p => p.UnidadFuncional)
            .OrderDescending()
            .ToListAsync();

        return View(pagosPropietario);
    }



    // GET: PAGOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (rol == "Administrador")
        {
            var pagoAdmin = await _context.Pagos
                .Include(p => p.Expensa)
                .Include(p => p.UnidadFuncional)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pagoAdmin == null)
            {
                return NotFound();
            }

            return View(pagoAdmin);
        }

        if (rol == "Propietario")
        {
            int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");

            if (unidadFuncionalId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var pagoPropietario = await _context.Pagos
                .Include(p => p.Expensa)
                .Include(p => p.UnidadFuncional)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.UnidadFuncionalId == unidadFuncionalId.Value);

            if (pagoPropietario == null)
            {
                return NotFound();
            }

            return View(pagoPropietario);
        }

        return RedirectToAction("Login", "Auth");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null)
        {
            return NotFound();
        }

        pago.Estado = EstadoPago.Aprobado;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = pago.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null)
        {
            return NotFound();
        }

        pago.Estado = EstadoPago.Rechazado;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = pago.Id });
    }

    // GET: PAGOS/Create
    public async Task<IActionResult> Create()
    {
        int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");

        if (ufId == null)
            return RedirectToAction("Login", "Auth");

        await CargarExpensasAsync(ufId.Value);

        return View(new Pago
        {
            FechaPago = DateTime.Today
        });
    }

    // POST: PAGOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("ExpensaId,FechaPago,MontoPagado,MedioPago,NumeroOperacion,BancoEntidad,Comentarios")]
    Pago pago,
    IFormFile? comprobante)
    {
        int? ufId =
            HttpContext.Session.GetInt32("UnidadFuncionalId");

        if (ufId == null)
            return RedirectToAction("Login", "Auth");

        bool expensaValida = await _context.Expensas.AnyAsync(e =>
            e.Id == pago.ExpensaId &&
            e.UnidadFuncionalId == ufId.Value);

        if (!expensaValida)
        {
            ModelState.AddModelError(
                nameof(pago.ExpensaId),
                "La expensa seleccionada no es válida.");
        }

        if (ModelState.IsValid)
        {
            if (comprobante != null && comprobante.Length > 0)
            {
                pago.ComprobantePath =
                    await GuardarArchivoAsync(comprobante);
            }

            pago.UnidadFuncionalId = ufId.Value;
            pago.FechaCreacion = DateTime.UtcNow;
            pago.Estado = EstadoPago.PendienteRevision;

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await CargarExpensasAsync(
            ufId.Value,
            pago.ExpensaId);

        return View(pago);
    }

    private async Task CargarExpensasAsync(
    int unidadFuncionalId,
    int? seleccionada = null)
    {
        var expensas = await _context.Expensas
            .Where(e => e.UnidadFuncionalId == unidadFuncionalId)
            .OrderByDescending(e => e.FechaEmision)
            .Select(e => new
            {
                e.Id,
                e.Periodo,
                e.MontoTotal,
                Pagado = e.Pagos
                    .Where(p => p.Estado == EstadoPago.Aprobado)
                    .Sum(p => (decimal?)p.MontoPagado) ?? 0
            })
            .ToListAsync();

        ViewBag.Expensas = new SelectList(
            expensas,
            "Id",
            "Periodo",
            seleccionada);

        ViewBag.ExpensasInfo = expensas.ToDictionary(
            e => e.Id,
            e => new
            {
                monto = e.MontoTotal,
                pagado = e.Pagado,
                saldo = Math.Max(e.MontoTotal - e.Pagado, 0)
            });
    }


    // GET: PAGOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");

        if (id == null || ufId == null)
            return NotFound();

        var pago = await _context.Pagos.FirstOrDefaultAsync(p =>
        p.Id == id &&
        p.UnidadFuncionalId == ufId.Value);

        if (pago == null)
        {
            return NotFound();
        }
        await CargarExpensasAsync(ufId.Value, pago.ExpensaId);
        return View(pago);
    }

    // POST: PAGOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ExpensaId,FechaPago,MontoPagado,MedioPago,NumeroOperacion,BancoEntidad,ComprobantePath,Comentarios")] Pago datos)
    {

        int? ufId = HttpContext.Session.GetInt32("UnidadFuncionalId");
        if (ufId == null || id != datos.Id)
            return NotFound();

        var pago = await _context.Pagos.FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.UnidadFuncionalId == ufId.Value);

        if (pago == null)
            return NotFound();

        bool expensaValida = await _context.Expensas.AnyAsync(e =>
            e.Id == datos.ExpensaId &&
            e.UnidadFuncionalId == ufId.Value);

        if (!expensaValida)
            ModelState.AddModelError("ExpensaId", "La expensa seleccionada no es válida.");

        if (ModelState.IsValid)
        {
            pago.ExpensaId = datos.ExpensaId;
            pago.FechaPago = datos.FechaPago;
            pago.MontoPagado = datos.MontoPagado;
            pago.MedioPago = datos.MedioPago;
            pago.NumeroOperacion = datos.NumeroOperacion;
            pago.BancoEntidad = datos.BancoEntidad;
            pago.ComprobantePath = datos.ComprobantePath;
            pago.Comentarios = datos.Comentarios;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await CargarExpensasAsync(ufId.Value, datos.ExpensaId);
        return View(datos);
    }

    // GET: PAGOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pago = await _context.Pagos
            .FirstOrDefaultAsync(m => m.Id == id);
        if (pago == null)
        {
            return NotFound();
        }

        return View(pago);
    }

    // POST: PAGOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        if (pago != null)
        {
            _context.Pagos.Remove(pago);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PagoExists(int? id)
    {
        return _context.Pagos.Any(e => e.Id == id);
    }

    private async Task<string> GuardarArchivoAsync(IFormFile archivo)
    {
        var carpeta = Path.Combine(
            _environment.WebRootPath,
            "uploads");

        Directory.CreateDirectory(carpeta);

        var extension = Path.GetExtension(archivo.FileName);
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using var stream =
            new FileStream(rutaFisica, FileMode.Create);

        await archivo.CopyToAsync(stream);

        return $"/uploads/{nombreArchivo}";
    }
}
