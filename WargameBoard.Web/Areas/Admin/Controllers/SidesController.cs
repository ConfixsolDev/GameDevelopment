using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Models;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SidesController : Controller
    {
        private readonly WargameDbContext _context;

        public SidesController(WargameDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Sides
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 25)
        {
            var query = _context.Sides.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString) || 
                                       (s.Description != null && s.Description.Contains(searchString)));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.Name)
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

        // GET: Admin/Sides/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var side = await _context.Sides.FirstOrDefaultAsync(m => m.Id == id);
            if (side == null)
            {
                return NotFound();
            }

            return View(side);
        }

        // GET: Admin/Sides/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Sides/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Color,Description,IsActive")] Side side)
        {
            if (ModelState.IsValid)
            {
                _context.Add(side);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Side created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(side);
        }

        // GET: Admin/Sides/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var side = await _context.Sides.FindAsync(id);
            if (side == null)
            {
                return NotFound();
            }
            return View(side);
        }

        // POST: Admin/Sides/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Color,Description,IsActive")] Side side)
        {
            if (id != side.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(side);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Side updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SideExists(side.Id))
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
            return View(side);
        }

        // GET: Admin/Sides/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var side = await _context.Sides.FirstOrDefaultAsync(m => m.Id == id);
            if (side == null)
            {
                return NotFound();
            }

            return View(side);
        }

        // POST: Admin/Sides/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var side = await _context.Sides.FindAsync(id);
            if (side != null)
            {
                _context.Sides.Remove(side);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Side deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SideExists(int id)
        {
            return _context.Sides.Any(e => e.Id == id);
        }
    }
}
