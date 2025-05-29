using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models.ViewModels;
using SewingMaterialsStorage.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SewingMaterialsStorage.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialsController> _logger;
        public ReportsController(ApplicationDbContext context, ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
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

        // Отчет по движению (последние операции)
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

        // Отчет о расходах за период (новый отчет)
        [HttpPost]
        public async Task<IActionResult> ConsumptionReport(ConsumptionReportViewModel model)
        {
            _logger.LogInformation($"Start generating report from {model.StartDate} to {model.EndDate}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid");
                model.MaterialTypes = new SelectList(_context.MaterialTypes, "TypeId", "TypeName");
                return View(model);
            }

            if (model.StartDate > model.EndDate)
            {
                _logger.LogWarning("Start date is after end date");
                ModelState.AddModelError("", "Начальная дата не может быть позже конечной");
                model.MaterialTypes = new SelectList(_context.MaterialTypes, "TypeId", "TypeName");
                return View(model);
            }

            try
            {
                var query = _context.Consumptions
                    .Include(c => c.Material)
                    .ThenInclude(m => m.MaterialType)
                    .Where(c => c.ConsumptionDate >= model.StartDate &&
                               c.ConsumptionDate <= model.EndDate);

                if (model.MaterialTypeId.HasValue)
                {
                    query = query.Where(c => c.Material.MaterialType.TypeId == model.MaterialTypeId.Value);
                }

                var consumptions = await query.ToListAsync();
                _logger.LogInformation($"Found {consumptions.Count} consumption records");

                model.Items = consumptions
                    .GroupBy(c => new { c.Material.MaterialType.TypeName, c.Material.MaterialName, c.Material.PricePerUnit })
                    .Select(g => new ConsumptionReportItem
                    {
                        MaterialType = g.Key.TypeName,
                        MaterialName = g.Key.MaterialName,
                        Quantity = g.Sum(c => c.Quantity),
                        PricePerUnit = g.Key.PricePerUnit,
                        TotalAmount = g.Sum(c => c.Quantity * g.Key.PricePerUnit)
                    })
                    .OrderBy(i => i.MaterialType)
                    .ThenBy(i => i.MaterialName)
                    .ToList();

                _logger.LogInformation($"Generated {model.Items.Count} report items");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating consumption report");
                ModelState.AddModelError("", "Произошла ошибка при формировании отчета");
            }

            model.MaterialTypes = new SelectList(_context.MaterialTypes, "TypeId", "TypeName", model.MaterialTypeId);
            return View(model);
        }
    }
}