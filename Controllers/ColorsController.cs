using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;
using SewingMaterialsStorage.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class ColorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialsController> _logger;

        public ColorsController(ApplicationDbContext context,ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new ExcelImportViewModel { ImportType = ImportType.Colors });
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

                        // Получаем все существующие цвета для быстрого поиска
                        var existingColors = await _context.Colors.ToDictionaryAsync(c => c.ColorName, c => c);

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var colorName = worksheet.Cells[row, 1].Text.Trim();

                            if (string.IsNullOrWhiteSpace(colorName))
                            {
                                skippedCount++;
                                continue;
                            }

                            if (existingColors.TryGetValue(colorName, out var existingColor))
                            {
                                // Цвет уже существует - можно обновить, если есть другие поля
                                // В данном случае просто пропускаем, так как обновлять нечего
                                skippedCount++;
                            }
                            else
                            {
                                // Новый цвет - добавляем
                                var newColor = new Color { ColorName = colorName };
                                _context.Colors.Add(newColor);
                                addedCount++;
                                existingColors.Add(colorName, newColor); // Добавляем в словарь для последующих проверок
                            }
                        }

                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Импорт завершен. Добавлено: {addedCount}, обновлено: {updatedCount}, пропущено (уже существует): {skippedCount}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте цветов");
                TempData["ErrorMessage"] = "Ошибка при импорте: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Colors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Colors.ToListAsync());
        }

        // GET: Colors/GetColorsForSelect 
        [HttpGet]
        public async Task<IActionResult> GetColorsForSelect()
        {
            var colors = await _context.Colors
                .Select(c => new { id = c.ColorId, text = c.ColorName })
                .ToListAsync();
            return Json(colors);
        }
        // GET: Colors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var color = await _context.Colors
                .FirstOrDefaultAsync(m => m.ColorId == id);
            if (color == null)
            {
                return NotFound();
            }

            return View(color);
        }

        // GET: Colors/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Color color) 
        {
            if (ModelState.IsValid)
            {
                _context.Add(color);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(color);
        }

        // GET: Colors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var color = await _context.Colors.FindAsync(id);
            if (color == null)
            {
                return NotFound();
            }
            return View(color);
        }

        // POST: Colors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ColorId,ColorName")] Color color)
        {
            if (id != color.ColorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(color);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ColorExists(color.ColorId))
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
            return View(color);
        }

        // GET: Colors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var color = await _context.Colors
                .FirstOrDefaultAsync(m => m.ColorId == id);
            if (color == null)
            {
                return NotFound();
            }

            return View(color);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var color = await _context.Colors
                .Include(c => c.MaterialColors) // Загружаем связанные записи
                .FirstOrDefaultAsync(c => c.ColorId == id);

            if (color == null)
            {
                return NotFound();
            }

            // Удаляем сначала связанные записи
            _context.MaterialColors.RemoveRange(color.MaterialColors);

            // Затем удаляем сам цвет
            _context.Colors.Remove(color);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ColorExists(int id)
        {
            return _context.Colors.Any(e => e.ColorId == id);
        }
    }
}