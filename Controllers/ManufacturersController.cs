using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.Controllers
{
    public class ManufacturersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManufacturersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Manufacturers
        public async Task<IActionResult> Index()
        {
            var manufacturers = await _context.Manufacturers
                .Include(m => m.Country)
                .ToListAsync();
            return View(manufacturers);
        }

        // GET: Manufacturers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers
                .Include(m => m.Country)
                .FirstOrDefaultAsync(m => m.ManufacturerId == id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            return View(manufacturer);
        }

        // GET: Manufacturers/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName");
            return View();
        }

        // POST: Manufacturers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        [Bind("ManufacturerId,ManufacturerName,CountryId")] Manufacturer manufacturer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Добавьте лог для проверки получаемых данных
                    Console.WriteLine($"Создание производителя: {manufacturer.ManufacturerName}, CountryId: {manufacturer.CountryId}");

                    _context.Add(manufacturer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Улучшенное логирование ошибок
                    Console.WriteLine($"Ошибка при сохранении: {ex.Message}\n{ex.StackTrace}");
                    ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                }
            }

            // Важно: повторно заполняем SelectList при ошибке
            ViewData["CountryId"] = new SelectList(
                _context.Countries,
                "CountryId",
                "CountryName",
                manufacturer.CountryId
            );

            return View(manufacturer);
        }

        // GET: Manufacturers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", manufacturer.CountryId);
            return View(manufacturer);
        }

        // POST: Manufacturers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ManufacturerId,ManufacturerName,CountryId")] Manufacturer manufacturer)
        {
            if (id != manufacturer.ManufacturerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(manufacturer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManufacturerExists(manufacturer.ManufacturerId))
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
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", manufacturer.CountryId);
            return View(manufacturer);
        }

        // GET: Manufacturers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers
                .Include(m => m.Country)
                .FirstOrDefaultAsync(m => m.ManufacturerId == id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            return View(manufacturer);
        }

        // POST: Manufacturers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            _context.Manufacturers.Remove(manufacturer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ManufacturerExists(int id)
        {
            return _context.Manufacturers.Any(e => e.ManufacturerId == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetManufacturers()
        {
            var manufacturers = await _context.Manufacturers
                .Select(m => new { value = m.ManufacturerId, text = m.ManufacturerName })
                .ToListAsync();
            return Json(manufacturers);
        }
    }
}