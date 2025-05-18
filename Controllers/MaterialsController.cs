using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Materials
        public async Task<IActionResult> Index()
        {
            var materials = await _context.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Manufacturer)
                .ToListAsync();
            return View(materials);
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.MaterialTypes, "TypeId", "TypeName");
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "ManufacturerName");
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        public async Task<IActionResult> Create(
            Material material,
            List<int> selectedColors,
            List<int> selectedCompositions,
            MaterialFabric? fabricDetails,
            MaterialThread? threadDetails,
            MaterialZipper? zipperDetails,
            MaterialButton? buttonDetails)
        {
            if (ModelState.IsValid)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // Сохранение материала
                    _context.Materials.Add(material);
                    await _context.SaveChangesAsync();

                    // Привязка цветов
                    if (selectedColors != null)
                    {
                        foreach (var colorId in selectedColors)
                        {
                            _context.MaterialColors.Add(new MaterialColor
                            {
                                MaterialId = material.MaterialId,
                                ColorId = colorId
                            });
                        }
                    }

                    // Привязка составов
                    if (selectedCompositions != null)
                    {
                        foreach (var compositionId in selectedCompositions)
                        {
                            _context.MaterialCompositions.Add(new MaterialComposition
                            {
                                MaterialId = material.MaterialId,
                                CompositionId = compositionId
                            });
                        }
                    }

                    // Сохранение специфических характеристик
                    switch (material.TypeId)
                    {
                        case 1 when fabricDetails != null:
                            fabricDetails.MaterialId = material.MaterialId;
                            _context.MaterialFabrics.Add(fabricDetails);
                            break;

                        case 2 when threadDetails != null:
                            threadDetails.MaterialId = material.MaterialId;
                            _context.MaterialThreads.Add(threadDetails);
                            break;

                        case 3 when zipperDetails != null:
                            zipperDetails.MaterialId = material.MaterialId;
                            _context.MaterialZippers.Add(zipperDetails);
                            break;

                        case 4 when buttonDetails != null:
                            buttonDetails.MaterialId = material.MaterialId;
                            _context.MaterialButtons.Add(buttonDetails);
                            break;
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
                }
            }

            // Загрузка справочников при ошибке
            await LoadSelectListsAsync();
            return View(material);
        }

        private async Task LoadSelectListsAsync()
        {
            ViewBag.Manufacturers = new SelectList(
                await _context.Manufacturers.ToListAsync(),
                "ManufacturerId",
                "ManufacturerName"
            );

            ViewBag.Colors = new SelectList(
                await _context.Colors.ToListAsync(),
                "ColorId",
                "Name"
            );

            ViewBag.Compositions = new SelectList(
                await _context.Compositions.ToListAsync(),
                "CompositionId",
                "Name"
            );

            ViewBag.MaterialTypes = new SelectList(
                await _context.MaterialTypes.ToListAsync(),
                "TypeId",
                "TypeName"
            );
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

        [HttpGet]
        public IActionResult GetTypeFields(int typeId)
        {
            return typeId switch
            {
                1 => PartialView("Partials/_FabricFields", new MaterialFabric()),
                2 => PartialView("Partials/_ThreadFields", new MaterialThread()),
                3 => PartialView("Partials/_ZipperFields", new MaterialZipper()),
                4 => PartialView("Partials/_ButtonFields", new MaterialButton()),
                _ => Content("")
            };
        }
    }
}