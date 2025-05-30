using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Services;

namespace SewingMaterialsStorage.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CountriesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.OrderBy(c => c.CountryName).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            var filePath = Path.Combine(_env.WebRootPath, "data", "countries.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Файл countries.xlsx не найден";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var importService = new CountryImportService();
                var countries = importService.ImportCountriesFromExcel(filePath);

                _context.Countries.RemoveRange(_context.Countries);
                await _context.SaveChangesAsync();

                await _context.Countries.AddRangeAsync(countries);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Успешно импортировано {countries.Count} стран";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка импорта: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}