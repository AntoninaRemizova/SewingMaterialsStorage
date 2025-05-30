using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalMaterials = await _context.Materials.CountAsync(),

                FabricCount = await _context.Materials
                    .CountAsync(m => m.MaterialType.TypeName == "ткань"),

                ThreadCount = await _context.Materials
                    .CountAsync(m => m.MaterialType.TypeName == "нитки"),

                ZipperCount = await _context.Materials
                    .CountAsync(m => m.MaterialType.TypeName == "молния"),

                ButtonCount = await _context.Materials
                    .CountAsync(m => m.MaterialType.TypeName == "пуговица"),

                RecentSupplies = await _context.Supplies
                    .Include(s => s.Material)
                    .OrderByDescending(s => s.SupplyDate)
                    .Take(5)
                    .ToListAsync(),

                RecentConsumptions = await _context.Consumptions
                    .Include(c => c.Material)
                    .OrderByDescending(c => c.ConsumptionDate)
                    .Take(5)
                    .ToListAsync(),

                LowStockCount = await CalculateLowStockCount(),

                TotalInventoryValue = await CalculateTotalInventoryValue(),

                MaterialsBelowThreshold = await CalculateMaterialsBelowThreshold()
            };

            return View(model);
        }

        private async Task<int> CalculateLowStockCount()
        {
            return await _context.Materials
                .Where(m => (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity)) < m.MinThreshold)
                .CountAsync();
        }

        private async Task<decimal> CalculateTotalInventoryValue()
        {
            return await _context.Materials
                .SumAsync(m => m.PricePerUnit *
                    (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity)));
        }

        private async Task<int> CalculateMaterialsBelowThreshold()
        {
            return await _context.Materials
                .Where(m => (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity)) < m.MinThreshold)
                .CountAsync();
        }
    }
}