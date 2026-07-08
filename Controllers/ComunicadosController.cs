
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDeConsorcios_v2_MVC.Context;

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
        return View(await _context.Comunicados.ToListAsync());
    }

    // GET: COMUNICADOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comunicado = await _context.Comunicados
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
        return View();
    }

    // POST: COMUNICADOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ConsorcioId,Titulo,Mensaje,FechaPublicacion,ArchivoAdjuntoPath,Importante,Consorcio")] Comunicado comunicado)
    {
        if (ModelState.IsValid)
        {
            _context.Add(comunicado);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(comunicado);
    }

    // GET: COMUNICADOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comunicado = await _context.Comunicados.FindAsync(id);
        if (comunicado == null)
        {
            return NotFound();
        }
        return View(comunicado);
    }

    // POST: COMUNICADOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ConsorcioId,Titulo,Mensaje,FechaPublicacion,ArchivoAdjuntoPath,Importante,Consorcio")] Comunicado comunicado)
    {
        if (id != comunicado.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(comunicado);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComunicadoExists(comunicado.Id))
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
        return View(comunicado);
    }

    // GET: COMUNICADOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comunicado = await _context.Comunicados
            .FirstOrDefaultAsync(m => m.Id == id);
        if (comunicado == null)
        {
            return NotFound();
        }

        return View(comunicado);
    }

    // POST: COMUNICADOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var comunicado = await _context.Comunicados.FindAsync(id);
        if (comunicado != null)
        {
            _context.Comunicados.Remove(comunicado);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ComunicadoExists(int? id)
    {
        return _context.Comunicados.Any(e => e.Id == id);
    }
}
