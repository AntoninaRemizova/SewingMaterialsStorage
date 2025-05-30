using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models.ViewModels;
using SewingMaterialsStorage.ViewModels;
using System.Drawing;

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

        private IActionResult ExportToExcel<T>(IEnumerable<T> data, string worksheetName, string fileName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            worksheet.Cells["A1"].LoadFromCollection(data, true);

            using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column].AutoFilter = true;

            var dateColumns = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                .Select(p => Array.IndexOf(typeof(T).GetProperties(), p) + 1);

            foreach (var col in dateColumns)
            {
                worksheet.Column(col).Style.Numberformat.Format = "dd.MM.yyyy";
            }

            var numericColumns = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(int) || p.PropertyType == typeof(decimal) ||
                           p.PropertyType == typeof(double) || p.PropertyType == typeof(float))
                .Select(p => Array.IndexOf(typeof(T).GetProperties(), p) + 1);

            foreach (var col in numericColumns)
            {
                worksheet.Column(col).Style.Numberformat.Format = "#,##0";
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                      $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

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

        public async Task<IActionResult> ExportStockToExcel()
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

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Остатки материалов");

            worksheet.Cells[1, 1].Value = "ID материала";
            worksheet.Cells[1, 2].Value = "Наименование";
            worksheet.Cells[1, 3].Value = "Артикул";
            worksheet.Cells[1, 4].Value = "Тип материала";
            worksheet.Cells[1, 5].Value = "Поступления";
            worksheet.Cells[1, 6].Value = "Расход";
            worksheet.Cells[1, 7].Value = "Остаток";
            worksheet.Cells[1, 8].Value = "Мин. уровень";
            worksheet.Cells[1, 9].Value = "Статус";

            int row = 2;
            foreach (var item in reportData)
            {
                worksheet.Cells[row, 1].Value = item.MaterialId;
                worksheet.Cells[row, 2].Value = item.MaterialName;
                worksheet.Cells[row, 3].Value = item.Article;
                worksheet.Cells[row, 4].Value = item.MaterialType;
                worksheet.Cells[row, 5].Value = item.TotalSupplies;
                worksheet.Cells[row, 6].Value = item.TotalConsumptions;
                worksheet.Cells[row, 7].Value = item.Balance;
                worksheet.Cells[row, 8].Value = item.MinThreshold;
                worksheet.Cells[row, 9].Value = item.IsLowStock ? "Дефицит" : "Норма";

                if (item.IsLowStock)
                {
                    worksheet.Cells[row, 1, row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 200, 200));
                }

                row++;
            }

            worksheet.Column(1).Width = 10;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 20;

            for (int col = 5; col <= 8; col++)
            {
                worksheet.Column(col).Style.Numberformat.Format = "#,##0";
                worksheet.Column(col).Width = 12;
                worksheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            worksheet.Cells[1, 1, 1, 9].AutoFilter = true;

            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                      $"Остатки_материалов_{DateTime.Now:yyyyMMdd}.xlsx");
        }

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

        public async Task<IActionResult> ExportMovementsToExcel()
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

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Движения материалов");

            worksheet.Cells[1, 1].Value = "Дата";
            worksheet.Cells[1, 2].Value = "Тип операции";
            worksheet.Cells[1, 3].Value = "Материал";
            worksheet.Cells[1, 4].Value = "Количество";
            worksheet.Cells[1, 5].Value = "Заказ/Примечание";

            int row = 2;
            foreach (var item in combined)
            {
                worksheet.Cells[row, 1].Value = item.Date;
                worksheet.Cells[row, 2].Value = item.OperationType;
                worksheet.Cells[row, 3].Value = item.MaterialName;
                worksheet.Cells[row, 4].Value = item.Quantity;
                worksheet.Cells[row, 5].Value = item.OrderId ?? "-";

                if (item.OperationType == "Поступление")
                {
                    worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
                else
                {
                    worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                }

                row++;
            }

            worksheet.Column(1).Style.Numberformat.Format = "dd.MM.yyyy";
            worksheet.Column(1).Width = 12;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 30;
            worksheet.Column(4).Style.Numberformat.Format = "#,##0";
            worksheet.Column(4).Width = 12;
            worksheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Column(5).Width = 20;

            worksheet.Cells[1, 1, 1, 5].AutoFilter = true;

            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                      $"Движения_материалов_{DateTime.Now:yyyyMMdd}.xlsx");
        }

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