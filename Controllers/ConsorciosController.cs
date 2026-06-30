using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;

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
    public async Task<IActionResult> Create([Bind("Id,Nombre,Cuit,Direccion,Ciudad,CodigoPostal,CantidadPisos,Observaciones,Estado,FechaCreacion,UnidadesFuncionales")] Consorcio consorcio)
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
            consorcio.FechaCreacion = DateTime.UtcNow;
            _context.Add(consorcio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(consorcio);
    }

    // GET: CONSORCIOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consorcio = await _context.Consorcios.FindAsync(id);
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
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,Cuit,Direccion,Ciudad,CodigoPostal,CantidadPisos,Observaciones,Estado,FechaCreacion,UnidadesFuncionales")] Consorcio consorcio)
    {
        if (id != consorcio.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(consorcio);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsorcioExists(consorcio.Id))
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
        return View(consorcio);
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
