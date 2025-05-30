using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;
using SewingMaterialsStorage.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class CompositionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialsController> _logger;

        public CompositionsController(ApplicationDbContext context, ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new ExcelImportViewModel { ImportType = ImportType.Compositions });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ExcelImportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await model.ExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        int addedCount = 0;
                        int updatedCount = 0;
                        int skippedCount = 0;

                        var existingCompositions = await _context.Compositions
                            .ToDictionaryAsync(c => c.CompositionName, c => c);

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var compositionName = worksheet.Cells[row, 1].Text.Trim();

                            if (string.IsNullOrWhiteSpace(compositionName))
                            {
                                skippedCount++;
                                continue;
                            }

                            if (existingCompositions.TryGetValue(compositionName, out var existingComposition))
                            {
                                skippedCount++;
                            }
                            else
                            {
                                var newComposition = new Composition { CompositionName = compositionName };
                                _context.Compositions.Add(newComposition);
                                addedCount++;
                                existingCompositions.Add(compositionName, newComposition);
                            }
                        }

                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Импорт завершен. Добавлено: {addedCount}, обновлено: {updatedCount}, пропущено (уже существует): {skippedCount}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте составов");
                TempData["ErrorMessage"] = "Ошибка при импорте: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Compositions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Compositions.ToListAsync());
        }

        // GET: Compositions/GetCompositionsForSelect
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