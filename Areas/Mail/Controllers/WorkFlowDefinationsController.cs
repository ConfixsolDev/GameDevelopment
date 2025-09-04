using TechWebSol.Areas.Mail.Models;
using TechWebSol.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TechWebSol.Extensions;

namespace TechWebSol.Areas.Mail.Controllers
{

    [Area("Mail")]
    [Authorize]
    [DisplayName("Work Flow Managment: Workflow managment Permission(Admin)")]
    public class WorkFlowDefinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkFlowDefinationsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [DisplayName("Work Flow Managment: List Permissions")]
        public IActionResult Index()
        {
            return View();
        }

        [DisplayName("Work Flow Managment: List Permissions")]
        [HttpPost]
        public async Task<ActionResult> Index([FromForm] JQDTParams param)
        {
            var totalRecords = await _context.WorkFlowDefination.Where(x => x.IsDeleted == false).CountAsync();

            var list = _context.WorkFlowDefination
                                .OrderByDescending(x => x.CreatedDate)
                                .Where(x=>x.Id != Guid.Parse("528b4c43-296d-4547-b213-10fc65133842"))
                                .AsNoTracking()
                                .Select(x => new WorkFlowDefinationDTO()
                                {
                                    id = x.Id,
                                    name = x.Name,
                                    date = x.CreatedDate.Value.ToString("dd/MM/yyyy"),
                                    isActive = x.IsActive,
                                }).AsQueryable();

            // Do not change here
            var ListForFrontEnd = new Datatable<WorkFlowDefinationDTO>(list);

            var FilterItem = ListForFrontEnd.FilterRecords(param);

            return Ok(new { param.draw, recordsFiltered = FilterItem.Item1, recordsTotal = totalRecords, data = FilterItem.Item2 });
        }
        [DisplayName("Work Flow Managment Details Permissions")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workFlowDefination = await _context.WorkFlowDefination
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workFlowDefination == null)
            {
                return NotFound();
            }

            return View(workFlowDefination);
        }
        [DisplayName("Work Flow Managment Create Permissions")]
        public IActionResult Create()
        {
            //WorkFlowDefination obj = new WorkFlowDefination();
            //return PartialView(obj);
            return PartialView();
        }
       
        [DisplayName("Work Flow Managment Create Permissions")]
        [HttpPost]
        public async Task<IActionResult> Create(WorkFlowDefination model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return Ok(model);
                }
                else
                {
                    return BadRequest(ModelStateExtensions.GetModalErrors(ModelState).FirstOrDefault().Value);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [DisplayName("Work Flow Managment Edit Permissions")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.WorkFlowDefination == null)
            {
                return NotFound();
            }

            var workFlowDefination = await _context.WorkFlowDefination.Where(x => x.Id != Guid.Parse("528b4c43-296d-4547-b213-10fc65133842")).FirstOrDefaultAsync(x=>x.Id == id);
            if (workFlowDefination == null)
            {
                return NotFound();
            }
            return PartialView(workFlowDefination);
        }
        [DisplayName("Work Flow Managment Edit Permissions")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkFlowDefination model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Ok(model);
                }
                else
                {
                    return BadRequest(ModelStateExtensions.GetModalErrors(ModelState).FirstOrDefault().Value);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.WorkFlowDefination == null)
            {
                return NotFound();
            }

            var workFlowDefination = await _context.WorkFlowDefination
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workFlowDefination == null)
            {
                return NotFound();
            }

            return View(workFlowDefination);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.WorkFlowDefination == null)
            {
                return Problem("Entity set 'WebApplication3Context.WorkFlowDefination'  is null.");
            }
            var workFlowDefination = await _context.WorkFlowDefination.FindAsync(id);
            if (workFlowDefination != null)
            {
                _context.WorkFlowDefination.Remove(workFlowDefination);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkFlowDefinationExists(Guid id)
        {
            return (_context.WorkFlowDefination?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
