
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;
using Microsoft.AspNetCore.Mvc.Rendering; //agregado para usar SelectList

public class ComunicadosController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public ComunicadosController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: COMUNICADOS
    public async Task<IActionResult> Index()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol == "Administrador")
        {
            return RedirectToAction("IndexAdmin", "Comunicados");
        }

        if (rol == "Propietario")
        {
            return RedirectToAction("IndexPropietario", "Comunicados");
        }

        return RedirectToAction("Login", "Auth");
    }

    public async Task<IActionResult> IndexPropietario()
    {
        int? unidadFuncionalId = HttpContext.Session.GetInt32("UnidadFuncionalId");

        if (unidadFuncionalId == null)
            return RedirectToAction("Login", "Auth");

        var consorcioId = await _context.UnidadesFuncionales
        .Where(u => u.Id == unidadFuncionalId)
        .Select(u => u.ConsorcioId)
         .FirstOrDefaultAsync();


        var comunicados = await _context.Comunicados
            .Where(com => com.ConsorcioId == consorcioId)
            .Include(c => c.Consorcio)
            .ToListAsync();

        return View(comunicados);
    }

    public async Task<IActionResult> IndexAdmin()
    {
        var comunicados = await _context.Comunicados
        .Include(c => c.Consorcio)
        .ToListAsync();

        return View(comunicados);
    }

    // GET: COMUNICADOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
                if (id == null)
        {
            return NotFound();
        }

        var comunicado = await _context.Comunicados
            .Include(c => c.Consorcio)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (comunicado == null)
        {
            return NotFound();
        }

        return View(comunicado);
    }

    // GET: COMUNICADOS/Create
    public IActionResult Create()
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre");

        return View(new Comunicado
        {
            FechaPublicacion = DateTime.Today
        });
    }

    // POST: COMUNICADOS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ConsorcioId,Titulo,Mensaje,FechaPublicacion,ArchivoAdjuntoPath,Importante")] Comunicado comunicado)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        ModelState.Remove("Consorcio");

        if (ModelState.IsValid)
        {
            _context.Comunicados.Add(comunicado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", comunicado.ConsorcioId);
        return View(comunicado);
    }

    // GET: COMUNICADOS/Edit/5
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

        var comunicado = await _context.Comunicados.FindAsync(id);

        if (comunicado == null)
        {
            return NotFound();
        }

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", comunicado.ConsorcioId);

        return View(comunicado);
    }

    // POST: COMUNICADOS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ConsorcioId,Titulo,Mensaje,FechaPublicacion,ArchivoAdjuntoPath,Importante")] Comunicado datos)
    {
        var rol = HttpContext.Session.GetString("UsuarioRol");

        if (rol != "Administrador")
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id != datos.Id)
        {
            return NotFound();
        }

        ModelState.Remove("Consorcio");

        if (ModelState.IsValid)
        {
            var comunicado = await _context.Comunicados.FindAsync(id);

            if (comunicado == null)
            {
                return NotFound();
            }

            comunicado.ConsorcioId = datos.ConsorcioId;
            comunicado.Titulo = datos.Titulo;
            comunicado.Mensaje = datos.Mensaje;
            comunicado.FechaPublicacion = datos.FechaPublicacion;
            comunicado.ArchivoAdjuntoPath = datos.ArchivoAdjuntoPath;
            comunicado.Importante = datos.Importante;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Consorcios = new SelectList(_context.Consorcios, "Id", "Nombre", datos.ConsorcioId);
        return View(datos);
    }

    private bool ComunicadoExists(int? id)
    {
        return _context.Comunicados.Any(e => e.Id == id);
    }
}