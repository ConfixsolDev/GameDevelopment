using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Models;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UnitTypesController : Controller
    {
        private readonly WargameDbContext _context;

        public UnitTypesController(WargameDbContext context)
        {
            _context = context;
        }

        // GET: Admin/UnitTypes
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 25)
        {
            var query = _context.UnitTypes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Name.Contains(searchString) || 
                                       (u.Description != null && u.Description.Contains(searchString)) ||
                                       (u.Category != null && u.Category.Contains(searchString)));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Category)
                .ThenBy(u => u.Name)
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

        // GET: Admin/UnitTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unitType = await _context.UnitTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (unitType == null)
            {
                return NotFound();
            }

            return View(unitType);
        }

        // GET: Admin/UnitTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/UnitTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Category,IsActive")] UnitType unitType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unitType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Unit type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(unitType);
        }

        // GET: Admin/UnitTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unitType = await _context.UnitTypes.FindAsync(id);
            if (unitType == null)
            {
                return NotFound();
            }
            return View(unitType);
        }

        // POST: Admin/UnitTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,IsActive")] UnitType unitType)
        {
            if (id != unitType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unitType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Unit type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnitTypeExists(unitType.Id))
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
            return View(unitType);
        }

        // GET: Admin/UnitTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unitType = await _context.UnitTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (unitType == null)
            {
                return NotFound();
            }

            return View(unitType);
        }

        // POST: Admin/UnitTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unitType = await _context.UnitTypes.FindAsync(id);
            if (unitType != null)
            {
                _context.UnitTypes.Remove(unitType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Unit type deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UnitTypeExists(int id)
        {
            return _context.UnitTypes.Any(e => e.Id == id);
        }
    }
}
