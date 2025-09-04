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
    [DisplayName("Lov: Degree Type")]
    public class DegreeTypeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGetSetupTypeLists _app;

        public DegreeTypeController(
            ApplicationDbContext context, IGetSetupTypeLists app
            )
        {
            _context = context;
            _app = app;
        }

        [DisplayName("Lov: Degree Type List Permissions")]
        public IActionResult Index()
        {
            var objs = _app.GetDegreeTypeSelectList().ToList();
            return View(objs);
        }
        [DisplayName("Lov: Degree Type Create Permissions")]
        [HttpGet]
        public IActionResult Create(int? id)
        {
            DegreeType newObj = new DegreeType();

            if (id != null && id != 0)
            {
                newObj = _context.DegreeType.Where(x => x.Id.Equals(id)).FirstOrDefault();
            }
            if (newObj == null)
            {
                return NotFound();
            }
            return PartialView(newObj);
        }
        [DisplayName("Lov: Degree Type Create Permissions")]
        [HttpPost]
        public async Task<IActionResult> Create(DegreeType model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id == 0)
                    {
                        if (await _context.DegreeType.AnyAsync(x => x.Name.Equals(model.Name)))
                        {
                            return BadRequest("List value already Exists");
                        }
                        await _context.DegreeType.AddAsync(model);
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
        [DisplayName("Lov: Degree Type Delete Permissions")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var deleteObj = await _context.DegreeType.Where(x => x.Id.Equals(Id)).FirstOrDefaultAsync();
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