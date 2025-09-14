using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Models;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ObstacleTypesController : Controller
    {
        private readonly WargameDbContext _context;

        public ObstacleTypesController(WargameDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 25)
        {
            var query = _context.ObstacleTypes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.Name.Contains(searchString) || 
                                       (o.Description != null && o.Description.Contains(searchString)));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(o => o.Name)
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
            var item = await _context.ObstacleTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,MovementPenalty,LineOfSightBlock,IsActive")] ObstacleType item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Obstacle type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ObstacleTypes.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,MovementPenalty,LineOfSightBlock,IsActive")] ObstacleType item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Obstacle type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObstacleTypeExists(item.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ObstacleTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ObstacleTypes.FindAsync(id);
            if (item != null)
            {
                _context.ObstacleTypes.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Obstacle type deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ObstacleTypeExists(int id) => _context.ObstacleTypes.Any(e => e.Id == id);
    }
}
