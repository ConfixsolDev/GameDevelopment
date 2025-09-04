using TechWebSol.Data;
using TechWebSol.Areas.Mail.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TechWebSol.Extensions;
using TechWebSol.Services;
namespace TechWebSol.Areas.Mail.Controllers
{
    [Area("Mail")]
    [DisplayName("Work Nodes: Work flow nodes permissions")]
    public class WorkFlowStepsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGetSetupTypeLists _GetSetupTypeLists;

        public WorkFlowStepsController(ApplicationDbContext context,
            IGetSetupTypeLists GetSetupTypeLists)
        {
            _context = context;
            _GetSetupTypeLists = GetSetupTypeLists;

        }

        [DisplayName("Work Nodes: List Permissions")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: api/PurchaseOrderLine
        [HttpGet]
        [AllowAnonymous]
        [DisplayName("Work Nodes: Get Workflow steps")]
        public IActionResult GetWorkFlowSteps(Guid? masterid)
        {
            var data = _context.WorkFlowStep
                .Include(x => x.ApplicationUserApp)
                .Include(w => w.WorkFlowDefination)
                .Where(x => x.WorkFlowDefinationId.Equals(masterid)).OrderBy(x => x.StepSequence).ToList();
            return Json(new { data });
        }

        [AllowAnonymous]
        [DisplayName("Work Nodes: Get Workflow steps")]
        public async Task<IActionResult> PostWorkFlowSteps([FromBody] WorkFlowStep workFlowStep)
        {
            if (ModelState.IsValid)
            {
                workFlowStep.Id = Guid.NewGuid();
                _context.Add(workFlowStep);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Add new data success." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Json(new { success = true, message = "Edit data success." });
        }

        [AllowAnonymous]
        [DisplayName("Work Nodes: Get Workflow steps")]
        public async Task<IActionResult> PostEditWorkFlowSteps([FromBody] WorkFlowStep workFlowStep)
        {
            if (ModelState.IsValid)
            {
                _context.Update(workFlowStep);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Edit data success." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Json(new { success = true, message = "Edit data success." });
        }


        [HttpPost("ChangeColumnOrder")]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeColumnOrder(ColumnSortOrderVM model)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var rows = _context.WorkFlowStep.Where(x => x.WorkFlowDefinationId == model.FileId);
                    var item = _context.WorkFlowStep.FirstOrDefault(x => x.Id == model.Id);

                    if (model.Action == "sort-up" && item != null)
                    {
                        var itemsToUpdate = rows.Where(m => m.StepSequence == (item.StepSequence - 1) && m.Id != item.Id).ToList();
                        // Perform actions on itemsToUpdate
                        itemsToUpdate.ForEach(m =>
                        {
                            m.StepSequence = item.StepSequence;
                        });

                        item.StepSequence = item.StepSequence - 1;

                        await _context.SaveChangesAsync();
                    }
                    else if (model.Action == "sort-down")
                    {
                        var itemsToUpdate = rows.Where(m => m.StepSequence == (item.StepSequence + 1) && m.Id != item.Id).ToList();
                        itemsToUpdate.ForEach(m =>
                        {
                            m.StepSequence = item.StepSequence;
                        });

                        item.StepSequence = item.StepSequence + 1;
                        await _context.SaveChangesAsync();
                    }
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.WorkFlowStep == null)
            {
                return NotFound();
            }

            var workFlowStep = await _context.WorkFlowStep
                .Include(w => w.ApplicationUserApp)
                .Include(w => w.WorkFlowDefination)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workFlowStep == null)
            {
                return NotFound();
            }

            return View(workFlowStep);
        }

        public IActionResult Create(Guid masterid, Guid? id)
        {
            var check = _context.WorkFlowStep.SingleOrDefault(m => m.Id == id);
            var selected = _context.WorkFlowDefination.SingleOrDefault(m => m.Id == masterid);
            //ViewData["applicationUserAppID"] = new SelectList(_context.ApplicationUser, "Id", "UserName");
            var userList = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.UserName} | {u.Designation}"
                })
                .ToList();

            ViewData["applicationUserAppID"] = new SelectList(userList, "Value", "Text");
            ViewData["workFlowDefinationId"] = new SelectList(_context.WorkFlowDefination, "Id", "Name");
            ViewData["stepsName"] = _GetSetupTypeLists.GetWorkFlowStepNameSelectList();
            if (check == null)
            {
                WorkFlowStep objline = new WorkFlowStep();
                objline.WorkFlowDefination = selected;
                objline.WorkFlowDefinationId = masterid;
                return PartialView(objline);
            }
            else
            {
                return PartialView(check);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkFlowStep model)
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

        // GET: InternalMail/WorkFlowSteps/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.WorkFlowStep == null)
            {
                return NotFound();
            }

            var workFlowStep = await _context.WorkFlowStep.FindAsync(id);
            if (workFlowStep == null)
            {
                return NotFound();
            }
            var userList = _context.Users
              .Select(u => new SelectListItem
              {
                  Value = u.Id.ToString(),
                  Text = $"{u.UserName} | {u.Designation}"
              })
              .ToList();

            ViewData["applicationUserAppID"] = new SelectList(userList, "Value", "Text");
            ViewData["workFlowDefinationId"] = new SelectList(_context.WorkFlowDefination, "Id", "Name");
            ViewData["stepsName"] = _GetSetupTypeLists.GetWorkFlowStepNameSelectList();

            return PartialView(workFlowStep);
        }
  
        [HttpPost]
        public async Task<IActionResult> Edit(WorkFlowStep model)
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

        //public async Task<IActionResult> Delete(Guid? id)
        //{
        //    if (id == null || _context.WorkFlowStep == null)
        //    {
        //        return NotFound();
        //    }

        //    var workFlowStep = await _context.WorkFlowStep
        //        .Include(w => w.ApplicationUserApp)
        //        .Include(w => w.WorkFlowDefination)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (workFlowStep == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(workFlowStep);
        //}

        //// POST: InternalMail/WorkFlowSteps/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(Guid id)
        //{
        //    if (_context.WorkFlowStep == null)
        //    {
        //        return Problem("Entity set 'WebApplication3Context.WorkFlowStep'  is null.");
        //    }
        //    var workFlowStep = await _context.WorkFlowStep.FindAsync(id);
        //    if (workFlowStep != null)
        //    {
        //        _context.WorkFlowStep.Remove(workFlowStep);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        public async Task<IActionResult> Delete([FromRoute] Guid? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var workFlowStep = await _context.WorkFlowStep.FindAsync(id);
            if (workFlowStep != null)
            {
                //personalInfo.IsDeleted = true;
                _context.Remove(workFlowStep);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = $"Removed successfully " });
        }
        private bool WorkFlowStepExists(Guid id)
        {
            return (_context.WorkFlowStep?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
