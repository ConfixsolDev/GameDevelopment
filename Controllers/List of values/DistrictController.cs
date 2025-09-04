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
    [DisplayName("Lov: District Type")]
    public class DistrictController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGetSetupTypeLists _app;

        public DistrictController(
            ApplicationDbContext context, IGetSetupTypeLists app
            )
        {
            _context = context;
            _app = app;
        }

        private void FillDropdownListWithData()
        {
            ViewData["ProvinceType"] = _app.GetProvinceList();
        }
        [DisplayName("Lov: District Type List Permissions")]
        public IActionResult Index()
        {
            var objs = _app.GetDistrictSelectList().ToList();
            return View(objs);
        }
        [DisplayName("Lov: District Type Create Permissions")]
        [HttpGet]
        public IActionResult Create(int? id)
        {
            District newObj = new District();

            if (id != null && id != 0)
            {
                newObj = _context.District.Where(x => x.Id.Equals(id)).FirstOrDefault();
            }
            if (newObj == null)
            {
                return NotFound();
            }
            FillDropdownListWithData();
            return PartialView(newObj);
        }
        [DisplayName("Lov: District Type Create Permissions")]
        [HttpPost]
        public async Task<IActionResult> Create(District model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id == 0)
                    {
                        if (await _context.District.AnyAsync(x => x.Name.Equals(model.Name)))
                        {
                            return BadRequest("List value already Exists");
                        }
                        await _context.District.AddAsync(model);
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
        [DisplayName("Lov: District Type Delete Permissions")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var deleteObj = await _context.District.Where(x => x.Id.Equals(Id)).FirstOrDefaultAsync();
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