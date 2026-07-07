using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;
using System.Linq;
using System.Collections.Generic;

public class ConsorciosController : Controller
{
    private readonly GestionDeConsorciosContext _context;

    public ConsorciosController(GestionDeConsorciosContext context)
    {
        _context = context;
    }

    // GET: CONSORCIOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Consorcios.ToListAsync());
    }

    // GET: CONSORCIOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consorcio = await _context.Consorcios
            .Include(c => c.UnidadesFuncionales)
            .Include(c => c.Amenities)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (consorcio == null)
        {
            return NotFound();
        }

        return View(consorcio);
    }

    // GET: CONSORCIOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CONSORCIOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,Cuit,Direccion,Ciudad,CodigoPostal,CantidadPisos,Observaciones,Estado,FechaCreacion,UnidadesFuncionales,Amenities")] Consorcio consorcio)
    {
        if (ModelState.IsValid)
        {
            if (consorcio.UnidadesFuncionales != null)
            {
                foreach (var uf in consorcio.UnidadesFuncionales)
                {
                    uf.Consorcio = consorcio;
                }
            }
            if (consorcio.Amenities != null)
            {
                foreach (var am in consorcio.Amenities)
                {
                    am.Consorcio = consorcio;
                }
            }
            consorcio.FechaCreacion = DateTime.UtcNow;
            _context.Add(consorcio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // Recolectar errores para diagnóstico y mostrarlos en la vista
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        ViewBag.ModelErrors = errors;
        return View(consorcio);
    }

    // GET: CONSORCIOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }


        var consorcio = await _context.Consorcios
            .Include(c => c.UnidadesFuncionales)
            .Include(c => c.Amenities)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (consorcio == null)
        {
            return NotFound();
        }
        return View(consorcio);
    }

    // POST: CONSORCIOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,Cuit,Direccion,Ciudad,CodigoPostal,CantidadPisos,Observaciones,Estado,UnidadesFuncionales,Amenities")] Consorcio consorcio)
    {
        if (id != consorcio.Id)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.ModelErrors = errors;
            return View(consorcio);
        }

        // Cargar el consorcio existente con sus colecciones
        var existing = await _context.Consorcios
            .Include(c => c.UnidadesFuncionales)
            .Include(c => c.Amenities)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existing == null)
            return NotFound();

        // Actualizar propiedades escalares
        existing.Nombre = consorcio.Nombre;
        existing.Cuit = consorcio.Cuit;
        existing.Direccion = consorcio.Direccion;
        existing.Ciudad = consorcio.Ciudad;
        existing.CodigoPostal = consorcio.CodigoPostal;
        existing.CantidadPisos = consorcio.CantidadPisos;
        existing.Observaciones = consorcio.Observaciones;
        existing.Estado = consorcio.Estado;

        // Sincronizar Unidades Funcionales
        var postedUfs = consorcio.UnidadesFuncionales ?? new List<UnidadFuncional>();
        var existingUfs = existing.UnidadesFuncionales ?? new List<UnidadFuncional>();

        // Update or add
        foreach (var uf in postedUfs)
        {
            if (uf.Id == 0)
            {
                uf.Consorcio = existing;
                existing.UnidadesFuncionales.Add(uf);
            }
            else
            {
                var ex = existingUfs.FirstOrDefault(x => x.Id == uf.Id);
                if (ex != null)
                {
                    ex.NumeroUF = uf.NumeroUF;
                    ex.Piso = uf.Piso;
                    ex.Departamento = uf.Departamento;
                    ex.NombrePropietario = uf.NombrePropietario;
                    ex.MailPropietario = uf.MailPropietario;
                    ex.DniPropietario = uf.DniPropietario;
                    ex.Telefono = uf.Telefono;
                }
            }
        }

        // Remove deleted
        var postedUfIds = postedUfs.Where(u => u.Id != 0).Select(u => u.Id).ToHashSet();
        foreach (var ex in existingUfs.ToList())
        {
            if (!postedUfIds.Contains(ex.Id))
            {
                _context.UnidadesFuncionales.Remove(ex);
            }
        }

        var postedAms = consorcio.Amenities ?? new List<Amenity>();
        var existingAms = existing.Amenities?.ToList() ?? new List<Amenity>();

        foreach (var am in postedAms)
        {
            if (am.Id == 0)
            {
                am.Consorcio = existing;
                existing.Amenities.Add(am);
            }
            else
            {
                var ex = existingAms.FirstOrDefault(x => x.Id == am.Id);
                if (ex != null)
                {
                    ex.Nombre = am.Nombre;
                    ex.Descripcion = am.Descripcion;
                    ex.Capacidad = am.Capacidad;
                }
            }
        }

        var postedAmIds = postedAms.Where(a => a.Id != 0).Select(a => a.Id).ToHashSet();
        foreach (var ex in existingAms.ToList())
        {
            if (!postedAmIds.Contains(ex.Id))
            {
                _context.Amenities.Remove(ex);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ConsorcioExists(existing.Id))
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

    // GET: CONSORCIOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consorcio = await _context.Consorcios
            .FirstOrDefaultAsync(m => m.Id == id);
        if (consorcio == null)
        {
            return NotFound();
        }

        return View(consorcio);
    }

    // POST: CONSORCIOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var consorcio = await _context.Consorcios.FindAsync(id);
        if (consorcio != null)
        {
            _context.Consorcios.Remove(consorcio);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ConsorcioExists(int? id)
    {
        return _context.Consorcios.Any(e => e.Id == id);
    }
}
