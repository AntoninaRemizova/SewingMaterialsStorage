using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;
using SewingMaterialsStorage.ViewModels;

namespace SewingMaterialsStorage.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialsController> _logger;

        public MaterialsController(
            ApplicationDbContext context,
            ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Materials
        public async Task<IActionResult> Index(
            string searchString,
            int? typeId,
            int? manufacturerId,
            string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TypeFilter"] = typeId;
            ViewData["ManufacturerFilter"] = manufacturerId;
            ViewData["CurrentSort"] = sortOrder;

            // Загружаем списки для фильтров
            ViewBag.MaterialTypes = new SelectList(await _context.MaterialTypes.ToListAsync(), "TypeId", "TypeName");
            ViewBag.Manufacturers = new SelectList(await _context.Manufacturers.ToListAsync(), "ManufacturerId", "ManufacturerName");

            var materials = _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(searchString))
            {
                materials = materials.Where(m => m.MaterialName.Contains(searchString) ||
                                     (m.Article != null && m.Article.Contains(searchString)));
            }

            if (typeId.HasValue)
            {
                materials = materials.Where(m => m.TypeId == typeId.Value);
            }

            if (manufacturerId.HasValue)
            {
                materials = materials.Where(m => m.ManufacturerId == manufacturerId.Value);
            }

            // Сортировка
            ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSort"] = sortOrder == "price" ? "price_desc" : "price";

            materials = sortOrder switch
            {
                "name_desc" => materials.OrderByDescending(m => m.MaterialName),
                "price" => materials.OrderBy(m => m.PricePerUnit),
                "price_desc" => materials.OrderByDescending(m => m.PricePerUnit),
                _ => materials.OrderBy(m => m.MaterialName)
            };

            return View(await materials.ToListAsync());
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .Include(m => m.FabricDetails)
                .Include(m => m.ThreadDetails)
                .Include(m => m.ZipperDetails)
                .Include(m => m.ButtonDetails)
                .Include(m => m.Colors).ThenInclude(mc => mc.Color)
                .Include(m => m.Compositions).ThenInclude(mc => mc.Composition)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();
            return View(material);
        }

        // GET: Materials/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AllColors = await _context.Colors.ToListAsync();
            ViewBag.AllCompositions = await _context.Compositions.ToListAsync();
            await LoadSelectListsAsync();
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Создаем основной материал
                    var material = new Material
                    {
                        MaterialName = viewModel.MaterialName,
                        Article = viewModel.Article,
                        PricePerUnit = viewModel.PricePerUnit,
                        MinThreshold = viewModel.MinThreshold,
                        Notes = viewModel.Notes,
                        ManufacturerId = viewModel.ManufacturerId,
                        TypeId = viewModel.TypeId
                    };

                    _context.Add(material);
                    await _context.SaveChangesAsync();

                    // Добавляем цвета
                    if (viewModel.SelectedColors != null)
                    {
                        foreach (var colorId in viewModel.SelectedColors)
                        {
                            _context.MaterialColors.Add(new MaterialColor
                            {
                                MaterialId = material.MaterialId,
                                ColorId = colorId
                            });
                        }
                    }

                    // Добавляем составы (только для тканей)
                    if (viewModel.SelectedCompositions != null && viewModel.TypeId == 9)
                    {
                        foreach (var compositionId in viewModel.SelectedCompositions)
                        {
                            _context.MaterialCompositions.Add(new MaterialComposition
                            {
                                MaterialId = material.MaterialId,
                                CompositionId = compositionId
                            });
                        }
                    }

                    // Добавляем специфические данные
                    switch (viewModel.TypeId)
                    {
                        case 9: // Ткань
                            _context.MaterialFabrics.Add(new MaterialFabric
                            {
                                MaterialId = material.MaterialId,
                                Width = viewModel.Width ?? 0,
                                Density = viewModel.Density ?? 0
                            });
                            break;
                        case 10: // Нитки
                            _context.MaterialThreads.Add(new MaterialThread
                            {
                                MaterialId = material.MaterialId,
                                Thickness = viewModel.Thickness ?? 0,
                                LengthPerSpool = viewModel.LengthPerSpool ?? 0
                            });
                            break;
                        case 11: // Молния
                            _context.MaterialZippers.Add(new MaterialZipper
                            {
                                MaterialId = material.MaterialId,
                                ZipperType = viewModel.ZipperType ?? "Не указано",
                                ZipperLength = viewModel.ZipperLength ?? 0
                            });
                            break;
                        case 12: // Пуговица
                            _context.MaterialButtons.Add(new MaterialButton
                            {
                                MaterialId = material.MaterialId,
                                Shape = viewModel.Shape ?? "Не указано",
                                ButtonSize = viewModel.ButtonSize ?? 0
                            });
                            break;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Материал успешно добавлен";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении материала");
                    ModelState.AddModelError("", $"Произошла ошибка: {ex.Message}");
                }
            }
            else
            {
                // Логирование ошибок валидации
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                _logger.LogError("Ошибки валидации: " + string.Join("; ", errors));
            }

            await LoadSelectListsAsync();
            ViewBag.AllColors = await _context.Colors.ToListAsync();
            ViewBag.AllCompositions = await _context.Compositions.ToListAsync();
            return View(viewModel);
        }


        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials
                .Include(m => m.FabricDetails)
                .Include(m => m.ThreadDetails)
                .Include(m => m.ZipperDetails)
                .Include(m => m.ButtonDetails)
                .Include(m => m.Colors)
                .Include(m => m.Compositions)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            var viewModel = new MaterialViewModel
            {
                MaterialId = material.MaterialId,
                MaterialName = material.MaterialName,
                Article = material.Article,
                PricePerUnit = material.PricePerUnit,
                MinThreshold = material.MinThreshold,
                Notes = material.Notes,
                ManufacturerId = (int)material.ManufacturerId,
                TypeId = material.TypeId,
                SelectedColors = material.Colors.Select(c => c.ColorId).ToArray(),
                //SelectedCompositions = material.Compositions.Select(c => c.CompositionId).ToArray()
            };

            // Заполняем специфические поля в зависимости от типа
            switch (material.TypeId)
            {
                case 9: // Ткань
                    viewModel.Width = material.FabricDetails?.Width;
                    viewModel.Density = (int)material.FabricDetails?.Density;
                    viewModel.SelectedCompositions = material.Compositions.Select(c => c.CompositionId).ToArray();
                    break;
                case 10: // Нитки
                    viewModel.Thickness = material.ThreadDetails?.Thickness;
                    viewModel.LengthPerSpool = material.ThreadDetails?.LengthPerSpool;
                    break;
                case 11: // Молния
                    viewModel.ZipperType = material.ZipperDetails?.ZipperType;
                    viewModel.ZipperLength = material.ZipperDetails?.ZipperLength;
                    break;
                case 12: // Пуговица
                    viewModel.Shape = material.ButtonDetails?.Shape;
                    viewModel.ButtonSize = (int)material.ButtonDetails?.ButtonSize;
                    break;
            }

            await LoadSelectListsAsync();
            ViewBag.AllColors = await _context.Colors.ToListAsync();
            ViewBag.AllCompositions = await _context.Compositions.ToListAsync();

            return View(viewModel);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialViewModel viewModel)
        {
            if (id != viewModel.MaterialId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Обновляем основной материал
                    var material = await _context.Materials
                        .Include(m => m.FabricDetails)
                        .Include(m => m.ThreadDetails)
                        .Include(m => m.ZipperDetails)
                        .Include(m => m.ButtonDetails)
                        .Include(m => m.Colors)
                        .Include(m => m.Compositions)
                        .FirstOrDefaultAsync(m => m.MaterialId == id);

                    material.MaterialName = viewModel.MaterialName;
                    material.Article = viewModel.Article;
                    material.PricePerUnit = viewModel.PricePerUnit;
                    material.MinThreshold = viewModel.MinThreshold;
                    material.Notes = viewModel.Notes;
                    material.ManufacturerId = viewModel.ManufacturerId;
                    material.TypeId = viewModel.TypeId;

                    // Обновляем цвета
                    _context.MaterialColors.RemoveRange(material.Colors);
                    if (viewModel.SelectedColors != null)
                    {
                        foreach (var colorId in viewModel.SelectedColors)
                        {
                            _context.MaterialColors.Add(new MaterialColor
                            {
                                MaterialId = material.MaterialId,
                                ColorId = colorId
                            });
                        }
                    }

                    // Обновляем составы (только для тканей)
                    _context.MaterialCompositions.RemoveRange(material.Compositions);
                    if (viewModel.SelectedCompositions != null)
                    {
                        foreach (var compositionId in viewModel.SelectedCompositions)
                        {
                            _context.MaterialCompositions.Add(new MaterialComposition
                            {
                                MaterialId = material.MaterialId,
                                CompositionId = compositionId
                            });
                        }
                    }

                    // Обновляем специфические данные
                    switch (viewModel.TypeId)
                    {
                        case 9: // Ткань
                            if (material.FabricDetails == null)
                            {
                                material.FabricDetails = new MaterialFabric();
                                _context.MaterialFabrics.Add(material.FabricDetails);
                            }
                            material.FabricDetails.Width = viewModel.Width.Value;
                            material.FabricDetails.Density = viewModel.Density.Value;
                            break;
                        case 10: // Нитки
                            if (material.ThreadDetails == null)
                            {
                                material.ThreadDetails = new MaterialThread();
                                _context.MaterialThreads.Add(material.ThreadDetails);
                            }
                            material.ThreadDetails.Thickness = viewModel.Thickness.Value;
                            material.ThreadDetails.LengthPerSpool = viewModel.LengthPerSpool.Value;
                            break;
                        case 11: // Молния
                            if (material.ZipperDetails == null)
                            {
                                material.ZipperDetails = new MaterialZipper();
                                _context.MaterialZippers.Add(material.ZipperDetails);
                            }
                            material.ZipperDetails.ZipperType = viewModel.ZipperType;
                            material.ZipperDetails.ZipperLength = viewModel.ZipperLength.Value;
                            break;
                        case 12: // Пуговица
                            if (material.ButtonDetails == null)
                            {
                                material.ButtonDetails = new MaterialButton();
                                _context.MaterialButtons.Add(material.ButtonDetails);
                            }
                            material.ButtonDetails.Shape = viewModel.Shape;
                            material.ButtonDetails.ButtonSize = viewModel.ButtonSize.Value;
                            break;
                    }

                    _context.Update(material);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Материал успешно обновлен";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении материала");
                    ModelState.AddModelError("", "Произошла ошибка при обновлении. Попробуйте еще раз.");
                }
            }

            await LoadSelectListsAsync();
            ViewBag.AllColors = await _context.Colors.ToListAsync();
            ViewBag.AllCompositions = await _context.Compositions.ToListAsync();
            return View(viewModel);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .Include(m => m.FabricDetails)
                .Include(m => m.ThreadDetails)
                .Include(m => m.ZipperDetails)
                .Include(m => m.ButtonDetails)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials
                .Include(m => m.FabricDetails)
                .Include(m => m.ThreadDetails)
                .Include(m => m.ZipperDetails)
                .Include(m => m.ButtonDetails)
                .Include(m => m.Colors)
                .Include(m => m.Compositions)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Удаляем связанные данные
                if (material.FabricDetails != null)
                    _context.MaterialFabrics.Remove(material.FabricDetails);
                if (material.ThreadDetails != null)
                    _context.MaterialThreads.Remove(material.ThreadDetails);
                if (material.ZipperDetails != null)
                    _context.MaterialZippers.Remove(material.ZipperDetails);
                if (material.ButtonDetails != null)
                    _context.MaterialButtons.Remove(material.ButtonDetails);

                _context.MaterialColors.RemoveRange(material.Colors);
                _context.MaterialCompositions.RemoveRange(material.Compositions);

                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Материал успешно удален";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при удалении материала");
                TempData["ErrorMessage"] = "Не удалось удалить материал";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task LoadSelectListsAsync()
        {
            ViewBag.Manufacturers = new SelectList(
                await _context.Manufacturers.ToListAsync(),
                "ManufacturerId", "ManufacturerName");

            ViewBag.MaterialTypes = new SelectList(
                await _context.MaterialTypes.ToListAsync(),
                "TypeId", "TypeName");
        }

        [HttpGet]
        public async Task<IActionResult> GetCompositionsForSelect(string searchTerm)
        {
            var compositions = await _context.Compositions
                .Where(c => string.IsNullOrEmpty(searchTerm) ||
                           c.CompositionName.Contains(searchTerm))
                .Select(c => new { id = c.CompositionId, text = c.CompositionName })
                .ToListAsync();

            return Json(compositions);
        }

        [HttpGet]
        public IActionResult GetTypeFields(int typeId)
        {
            _logger.LogInformation($"GetTypeFields called with typeId: {typeId}");
            try
            {
                var viewModel = new MaterialViewModel { TypeId = typeId };

                // Для типа "Ткань" загружаем составы
                if (typeId == 9)
                {
                    ViewBag.AllCompositions = _context.Compositions.ToList();
                    _logger.LogInformation($"Loaded {ViewBag.AllCompositions.Count} compositions for fabric");
                }

                return typeId switch
                {
                    9 => PartialView("Partials/_FabricFields", viewModel),
                    10 => PartialView("Partials/_ThreadFields", viewModel),
                    11 => PartialView("Partials/_ZipperFields", viewModel),
                    12 => PartialView("Partials/_ButtonFields", viewModel),
                    _ => Content("")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading type fields");
                return Content($"<div class='alert alert-danger'>Error loading fields: {ex.Message}</div>");
            }
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.MaterialId == id);
        }
    }
}