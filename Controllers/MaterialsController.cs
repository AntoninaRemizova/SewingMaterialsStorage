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
        public async Task<IActionResult> Index()
        {
            var materials = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .Include(m => m.Colors).ThenInclude(mc => mc.Color)
                .Include(m => m.Compositions).ThenInclude(mc => mc.Composition)
                .ToListAsync();
            return View(materials);
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
        public async Task<IActionResult> Create(CreateMaterialViewModel viewModel)
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

                    // Добавляем составы
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

                    // Добавляем специфические данные в зависимости от типа
                    switch (viewModel.TypeId)
                    {
                        case 9: // Ткань
                            if (viewModel.Width.HasValue || viewModel.Density.HasValue)
                            {
                                _context.MaterialFabrics.Add(new MaterialFabric
                                {
                                    MaterialId = material.MaterialId,
                                    Width = viewModel.Width.Value,
                                    Density = viewModel.Density.Value
                                });
                            }
                            break;
                        case 10: // Нитки
                            if (viewModel.Thickness.HasValue || viewModel.LengthPerSpool.HasValue)
                            {
                                _context.MaterialThreads.Add(new MaterialThread
                                {
                                    MaterialId = material.MaterialId,
                                    Thickness = viewModel.Thickness.Value,
                                    LengthPerSpool = viewModel.LengthPerSpool.Value
                                });
                            }
                            break;
                        case 11: // Молния
                            if (!string.IsNullOrEmpty(viewModel.ZipperType) || viewModel.ZipperLength.HasValue)
                            {
                                _context.MaterialZippers.Add(new MaterialZipper
                                {
                                    MaterialId = material.MaterialId,
                                    ZipperType = viewModel.ZipperType,
                                    ZipperLength = viewModel.ZipperLength.Value
                                });
                            }
                            break;
                        case 12: // Пуговица
                            if (!string.IsNullOrEmpty(viewModel.Shape) || viewModel.ButtonSize.HasValue)
                            {
                                _context.MaterialButtons.Add(new MaterialButton
                                {
                                    MaterialId = material.MaterialId,
                                    Shape = viewModel.Shape,
                                    ButtonSize = viewModel.ButtonSize.Value
                                });
                            }
                            break;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Материал успешно добавлен";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении материала");
                    ModelState.AddModelError("", "Произошла ошибка при сохранении. Попробуйте еще раз.");
                }
            }

            await LoadSelectListsAsync();
            ViewBag.AllColors = await _context.Colors.ToListAsync();
            ViewBag.AllCompositions = await _context.Compositions.ToListAsync();
            return View(viewModel);
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

        // GET: Materials/GetColorsForSelect - для Select2
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
                var viewModel = new CreateMaterialViewModel();

                // Для типа "Ткань" загружаем составы
                if (typeId == 9) // ID типа "Ткань"
                {
                    ViewBag.AllCompositions = _context.Compositions.ToList();
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

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.MaterialTypes, "TypeId", "TypeName", material.TypeId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "ManufacturerName", material.ManufacturerId);
            return View(material);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaterialId,MaterialName,Article,PricePerUnit,ManufacturerId,MinThreshold,Notes,TypeId")] Material material)
        {
            if (id != material.MaterialId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.MaterialId))
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
            ViewData["TypeId"] = new SelectList(_context.MaterialTypes, "TypeId", "TypeName", material.TypeId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "ManufacturerName", material.ManufacturerId);
            return View(material);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.MaterialId == id);
        }

        
    }
}