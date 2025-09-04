using TechWebSol.Data;
using TechWebSol.Extensions;
using TechWebSol.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TechWebSol.Filters;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    [Authorize]
    [DisplayName("Lov: Sect Type")]
    public class SectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGetSetupTypeLists _app;

        public SectController(
            ApplicationDbContext context, IGetSetupTypeLists app
            )
        {
            _context = context;
            _app = app;
        }
        [DisplayName("Lov: Sect Type List Permissions")]
        public IActionResult Index()
        {
            var objs = _app.GetSectSelectList().ToList();
            return View(objs);
        }
        [DisplayName("Lov: Sect Type Create Permissions")]
        [HttpGet]
        public IActionResult Create(int? id)
        {
            Sect newObj = new Sect();

            if (id != null && id != 0)
            {
                newObj = _context.Sect.Where(x => x.Id.Equals(id)).FirstOrDefault();
            }
            if (newObj == null)
            {
                return NotFound();
            }
            return PartialView(newObj);
        }
        [DisplayName("Lov: Sect Type Create Permissions")]
        [HttpPost]
        public async Task<IActionResult> Create(Sect model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id == 0)
                    {
                        if (await _context.Sect.AnyAsync(x => x.Name.Equals(model.Name)))
                        {
                            return BadRequest("List value already Exists");
                        }
                        await _context.Sect.AddAsync(model);
                    }
                    else
                    {
                        _context.Update(model);
                    }
                    await _context.SaveChangesAsync();
                    return Ok("List value updated");
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
        [DisplayName("Lov: Sect Type Delete Permissions")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var deleteObj = await _context.Sect.Where(x => x.Id.Equals(Id)).FirstOrDefaultAsync();
                if (deleteObj == null)
                {
                    return NotFound();
                }
                deleteObj.IsDeleted = true;
                _context.Update(deleteObj);
                await _context.SaveChangesAsync();
                return Ok("List value deleted");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}