using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.Controllers
{
    public class CompositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Compositions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Compositions.ToListAsync());
        }

        // GET: Compositions/GetCompositionsForSelect - новый метод для Select2
        [HttpGet]
        public async Task<IActionResult> GetCompositionsForSelect()
        {
            var compositions = await _context.Compositions
                .Select(c => new { id = c.CompositionId, text = c.CompositionName })
                .ToListAsync();
            return Json(compositions);
        }

        // GET: Compositions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var composition = await _context.Compositions
                .FirstOrDefaultAsync(m => m.CompositionId == id);
            if (composition == null)
            {
                return NotFound();
            }

            return View(composition);
        }

        // GET: Compositions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Compositions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Composition composition) 
        {
            if (ModelState.IsValid)
            {
                _context.Add(composition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(composition);
        }

        // GET: Compositions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var composition = await _context.Compositions.FindAsync(id);
            if (composition == null)
            {
                return NotFound();
            }
            return View(composition);
        }

        // POST: Compositions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompositionId,CompositionName")] Composition composition)
        {
            if (id != composition.CompositionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(composition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompositionExists(composition.CompositionId))
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
            return View(composition);
        }

        // GET: Compositions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var composition = await _context.Compositions
                .FirstOrDefaultAsync(m => m.CompositionId == id);
            if (composition == null)
            {
                return NotFound();
            }

            return View(composition);
        }

        // POST: Compositions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var composition = await _context.Compositions.FindAsync(id);
            _context.Compositions.Remove(composition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompositionExists(int id)
        {
            return _context.Compositions.Any(e => e.CompositionId == id);
        }
    }
}