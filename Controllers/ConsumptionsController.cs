using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;
using SewingMaterialsStorage.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class ConsumptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialsController> _logger;

        public ConsumptionsController(ApplicationDbContext context, ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<decimal> GetMaterialPrice(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            return material?.PricePerUnit ?? 0;
        }

        // GET: Consumptions
        public async Task<IActionResult> Index()
        {
            var consumptions = await _context.Consumptions
                .Include(c => c.Material)
                .ThenInclude(m => m.MaterialType)
                .OrderByDescending(c => c.ConsumptionDate)
                .ToListAsync();

            return View(consumptions);
        }

        // GET: Consumptions/Create
        public IActionResult Create()
        {
            var viewModel = new ConsumptionViewModel
            {
                Materials = new SelectList(_context.Materials.Include(m => m.MaterialType),
                                        "MaterialId", "MaterialName")
            };
            return View(viewModel);
        }

        // POST: Consumptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsumptionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var material = await _context.Materials.FindAsync(viewModel.MaterialId);
                if (material == null)
                {
                    ModelState.AddModelError("MaterialId", "Материал не найден");
                    viewModel.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName");
                    return View(viewModel);
                }

                var consumption = new Consumption
                {
                    MaterialId = viewModel.MaterialId,
                    Quantity = viewModel.Quantity,
                    ConsumptionDate = viewModel.ConsumptionDate,
                    OrderId = viewModel.OrderId
                };

                _context.Add(consumption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName");
            return View(viewModel);
        }

        // GET: Consumptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consumption = await _context.Consumptions.FindAsync(id);
            if (consumption == null) return NotFound();

            var material = await _context.Materials.FindAsync(consumption.MaterialId);

            var viewModel = new ConsumptionViewModel
            {
                ConsumptionId = consumption.ConsumptionId,
                MaterialId = consumption.MaterialId,
                Quantity = consumption.Quantity,
                ConsumptionDate = consumption.ConsumptionDate,
                OrderId = consumption.OrderId,
                TotalAmount = consumption.Quantity * material.PricePerUnit,
                Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName")
            };

            return View(viewModel);
        }

        // POST: Consumptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsumptionViewModel viewModel)
        {
            if (id != viewModel.ConsumptionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var consumption = await _context.Consumptions.FindAsync(viewModel.ConsumptionId);
                    if (consumption == null) return NotFound();

                    consumption.MaterialId = viewModel.MaterialId;
                    consumption.Quantity = viewModel.Quantity;
                    consumption.ConsumptionDate = viewModel.ConsumptionDate.Date;
                    consumption.OrderId = viewModel.OrderId;

                    _context.Update(consumption);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении расхода");
                    ModelState.AddModelError("", "Ошибка при сохранении изменений");
                }
            }

            viewModel.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName", viewModel.MaterialId);
            return View(viewModel);
        }

        // GET: Consumptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var consumption = await _context.Consumptions
                .Include(c => c.Material)
                .ThenInclude(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.ConsumptionId == id);

            if (consumption == null) return NotFound();

            return View(consumption);
        }

        // GET: Consumptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var consumption = await _context.Consumptions
                .Include(c => c.Material)
                .FirstOrDefaultAsync(m => m.ConsumptionId == id);

            if (consumption == null) return NotFound();

            return View(consumption);
        }

        // POST: Consumptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consumption = await _context.Consumptions.FindAsync(id);
            if (consumption != null)
            {
                _context.Consumptions.Remove(consumption);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ConsumptionExists(int id)
        {
            return _context.Consumptions.Any(e => e.ConsumptionId == id);
        }
    }
}