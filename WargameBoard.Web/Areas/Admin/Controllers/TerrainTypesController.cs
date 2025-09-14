using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Models;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TerrainTypesController : Controller
    {
        private readonly WargameDbContext _context;

        public TerrainTypesController(WargameDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 25)
        {
            var query = _context.TerrainTypes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.Name.Contains(searchString) || 
                                       (t.Description != null && t.Description.Contains(searchString)));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            ViewBag.PagingInfo = pagingInfo;
            ViewBag.SearchString = searchString;

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TerrainTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Color,MovementCost,DefenseModifier,IsActive")] TerrainType item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Terrain type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TerrainTypes.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Color,MovementCost,DefenseModifier,IsActive")] TerrainType item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Terrain type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TerrainTypeExists(item.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TerrainTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.TerrainTypes.FindAsync(id);
            if (item != null)
            {
                _context.TerrainTypes.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Terrain type deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TerrainTypeExists(int id) => _context.TerrainTypes.Any(e => e.Id == id);
    }
}
