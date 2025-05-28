using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;
using SewingMaterialsStorage.ViewModels;
using SewingMaterialsStorage.Controllers;

public class SuppliesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MaterialsController> _logger;

    public SuppliesController(ApplicationDbContext context, ILogger<MaterialsController> logger)
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

    // GET: Supplies
    public async Task<IActionResult> Index()
    {
        var supplies = await _context.Supplies
            .Include(s => s.Material)
            .ThenInclude(m => m.MaterialType)
            .ToListAsync();

        return View(supplies);
    }

    // GET: Supplies/Create
    public IActionResult Create()
    {
        var viewModel = new SupplyViewModel
        {
            Materials = new SelectList(_context.Materials.Include(m => m.MaterialType),
                                    "MaterialId", "MaterialName")
        };
        return View(viewModel);
    }

    // POST: Supplies/Create
    [HttpPost]
    public async Task<IActionResult> Create(SupplyViewModel viewModel)
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

            var supply = new Supply
            {
                MaterialId = viewModel.MaterialId,
                Quantity = viewModel.Quantity,
                SupplyDate = viewModel.SupplyDate
            };

            _context.Add(supply);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        viewModel.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName");
        return View(viewModel);
    }

    // GET: Supplies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var supply = await _context.Supplies.FindAsync(id);
        if (supply == null) return NotFound();

        var material = await _context.Materials.FindAsync(supply.MaterialId);

        var viewModel = new SupplyViewModel
        {
            SupplyId = supply.SupplyId,
            MaterialId = supply.MaterialId,
            Quantity = supply.Quantity,
            SupplyDate = supply.SupplyDate,
            TotalAmount = supply.Quantity * material.PricePerUnit,
            Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName")
        };

        return View(viewModel);
    }

    // POST: Supplies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplyViewModel viewModel)
    {
        if (id != viewModel.SupplyId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var supply = await _context.Supplies.FindAsync(viewModel.SupplyId);
                if (supply == null) return NotFound();

                // Явное обновление полей
                supply.MaterialId = viewModel.MaterialId;
                supply.Quantity = viewModel.Quantity;

                // Явное преобразование даты
                supply.SupplyDate = viewModel.SupplyDate.Date; // Берем только дату без времени

                _context.Update(supply);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении поступления");
                ModelState.AddModelError("", "Ошибка при сохранении изменений");
            }
        }

        viewModel.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName", viewModel.MaterialId);
        return View(viewModel);
    }

    private bool SupplyExists(int id)
    {
        return _context.Supplies.Any(e => e.SupplyId == id);
    }



    // GET: Supplies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var supply = await _context.Supplies
            .Include(s => s.Material)
            .FirstOrDefaultAsync(m => m.SupplyId == id);

        if (supply == null) return NotFound();

        return View(supply);
    }

    // POST: Supplies/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supply = await _context.Supplies.FindAsync(id);
        if (supply != null)
        {
            _context.Supplies.Remove(supply);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Supplies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var supply = await _context.Supplies
            .Include(s => s.Material)
            .ThenInclude(m => m.MaterialType)
            .FirstOrDefaultAsync(m => m.SupplyId == id);

        if (supply == null) return NotFound();

        return View(supply);
    }
}