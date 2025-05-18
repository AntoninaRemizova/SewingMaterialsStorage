using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Data;
using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.Controllers
{
    public class SuppliesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var supplies = await _context.Supplies
                .Include(s => s.Material)
                .ThenInclude(m => m.MaterialType)
                .OrderByDescending(s => s.SupplyDate)
                .ToListAsync();

            return View(supplies);
        }
    }

}
