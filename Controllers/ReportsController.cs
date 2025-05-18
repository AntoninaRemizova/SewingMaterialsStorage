using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Отчет по остаткам
        public async Task<IActionResult> Stock()
        {
            var reportData = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Supplies)
                .Include(m => m.Consumptions)
                .Select(m => new StockReportItem
                {
                    MaterialId = m.MaterialId,
                    MaterialName = m.MaterialName,
                    Article = m.Article,
                    MaterialType = m.MaterialType.TypeName,
                    TotalSupplies = m.Supplies.Sum(s => s.Quantity),
                    TotalConsumptions = m.Consumptions.Sum(c => c.Quantity),
                    Balance = m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity),
                    MinThreshold = m.MinThreshold,
                    IsLowStock = (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity)) < m.MinThreshold
                })
                .ToListAsync();

            return View(reportData);
        }

        // Отчет по движению
        public async Task<IActionResult> Movements()
        {
            var supplies = await _context.Supplies
                .Include(s => s.Material)
                .OrderByDescending(s => s.SupplyDate)
                .Take(100)
                .Select(s => new MovementReportItem
                {
                    Date = s.SupplyDate,
                    OperationType = "Поступление",
                    MaterialName = s.Material.MaterialName,
                    Quantity = s.Quantity,
                    OrderId = null
                })
                .ToListAsync();

            var consumptions = await _context.Consumptions
                .Include(c => c.Material)
                .OrderByDescending(c => c.ConsumptionDate)
                .Take(100)
                .Select(c => new MovementReportItem
                {
                    Date = c.ConsumptionDate,
                    OperationType = "Расход",
                    MaterialName = c.Material.MaterialName,
                    Quantity = c.Quantity,
                    OrderId = c.OrderId
                })
                .ToListAsync();

            var combined = supplies.Concat(consumptions)
                .OrderByDescending(x => x.Date)
                .ToList();

            return View(combined);
        }

        // Отчет по материалам ниже минимального уровня
        public async Task<IActionResult> LowStock()
        {
            var reportData = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Supplies)
                .Include(m => m.Consumptions)
                .Where(m => (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity)) < m.MinThreshold)
                .Select(m => new LowStockReportItem
                {
                    MaterialId = m.MaterialId,
                    MaterialName = m.MaterialName,
                    MaterialType = m.MaterialType.TypeName,
                    CurrentBalance = m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity),
                    MinThreshold = m.MinThreshold,
                    DeficitAmount = m.MinThreshold - (m.Supplies.Sum(s => s.Quantity) - m.Consumptions.Sum(c => c.Quantity))
                })
                .ToListAsync();

            return View(reportData);
        }
    }
}