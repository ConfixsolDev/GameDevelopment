using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WargameBoard.Core.Data;
using WargameBoard.Core.Entities;
using WargameBoard.Web.Models;
using WargameBoard.Web.Models.ViewModels;
using WargameBoard.Web.Services;

namespace WargameBoard.Web.Areas.Scenarios.Controllers
{
    [Area("Scenarios")]
    public class ScenariosController : Controller
    {
        private readonly WargameDbContext _context;
        private readonly ISelectListService _selectListService;

        public ScenariosController(WargameDbContext context, ISelectListService selectListService)
        {
            _context = context;
            _selectListService = selectListService;
        }

        // GET: Scenarios
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 25)
        {
            var query = _context.Scenarios.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString) || 
                                       (s.Notes != null && s.Notes.Contains(searchString)));
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

        // GET: Scenarios/Create
        public IActionResult Create()
        {
            var viewModel = new ScenarioEditVm();
            return View("Edit", viewModel);
        }

        // POST: Scenarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TurnLengthMinutes,MaxTurns,Weather,Notes")] ScenarioEditVm viewModel)
        {
            if (ModelState.IsValid)
            {
                var scenario = new Scenario
                {
                    Name = viewModel.Name,
                    TurnLengthMinutes = viewModel.TurnLengthMinutes,
                    MaxTurns = viewModel.MaxTurns,
                    Weather = viewModel.Weather,
                    Notes = viewModel.Notes
                };

                _context.Add(scenario);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Scenario created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View("Edit", viewModel);
        }

        // GET: Scenarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scenario = await _context.Scenarios.FindAsync(id);
            if (scenario == null)
            {
                return NotFound();
            }

            var viewModel = new ScenarioEditVm
            {
                Id = scenario.Id,
                Name = scenario.Name,
                TurnLengthMinutes = scenario.TurnLengthMinutes,
                MaxTurns = scenario.MaxTurns,
                Weather = scenario.Weather,
                Notes = scenario.Notes
            };

            return View(viewModel);
        }

        // POST: Scenarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TurnLengthMinutes,MaxTurns,Weather,Notes")] ScenarioEditVm viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var scenario = await _context.Scenarios.FindAsync(id);
                    if (scenario == null)
                    {
                        return NotFound();
                    }

                    scenario.Name = viewModel.Name;
                    scenario.TurnLengthMinutes = viewModel.TurnLengthMinutes;
                    scenario.MaxTurns = viewModel.MaxTurns;
                    scenario.Weather = viewModel.Weather;
                    scenario.Notes = viewModel.Notes;

                    _context.Update(scenario);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Scenario updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScenarioExists(viewModel.Id))
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
            return View(viewModel);
        }

        // GET: Scenarios/Units/5
        public async Task<IActionResult> Units(int id)
        {
            var scenario = await _context.Scenarios.FindAsync(id);
            if (scenario == null)
            {
                return NotFound();
            }

            var scenarioUnits = await _context.ScenarioUnits
                .Where(su => su.ScenarioId == id)
                .ToListAsync();

            var viewModels = scenarioUnits.Select(su => new ScenarioUnitEditVm
            {
                Id = su.Id,
                ScenarioId = su.ScenarioId,
                UnitId = su.UnitId,
                StartHexId = su.StartHexId,
                Steps = su.Steps,
                Posture = (int)su.Posture,
                Hidden = su.Hidden,
                Units = _selectListService.GetUnitsAsync().Result,
                Hexes = _selectListService.GetHexesAsync().Result
            }).ToList();

            ViewBag.ScenarioId = id;
            ViewBag.ScenarioName = scenario.Name;
            return View(viewModels);
        }

        // POST: Scenarios/AddUnit
        [HttpPost]
        public async Task<IActionResult> AddUnit(int scenarioId)
        {
            var viewModel = new ScenarioUnitEditVm
            {
                ScenarioId = scenarioId,
                Units = await _selectListService.GetUnitsAsync(),
                Hexes = await _selectListService.GetHexesAsync()
            };

            return PartialView("_ScenarioUnitRow", viewModel);
        }

        // POST: Scenarios/SaveUnit
        [HttpPost]
        public async Task<IActionResult> SaveUnit([FromBody] ScenarioUnitEditVm viewModel)
        {
            if (ModelState.IsValid)
            {
                var scenarioUnit = new ScenarioUnit
                {
                    ScenarioId = viewModel.ScenarioId,
                    UnitId = viewModel.UnitId,
                    StartHexId = viewModel.StartHexId,
                    Steps = viewModel.Steps,
                    Posture = (Posture)viewModel.Posture,
                    Hidden = viewModel.Hidden
                };

                _context.Add(scenarioUnit);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            viewModel.Units = await _selectListService.GetUnitsAsync();
            viewModel.Hexes = await _selectListService.GetHexesAsync();
            return PartialView("_ScenarioUnitRow", viewModel);
        }

        // POST: Scenarios/DeleteUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var scenarioUnit = await _context.ScenarioUnits.FindAsync(id);
            if (scenarioUnit != null)
            {
                _context.ScenarioUnits.Remove(scenarioUnit);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Unit not found" });
        }

        // GET: Scenarios/Objectives/5
        public async Task<IActionResult> Objectives(int id)
        {
            var scenario = await _context.Scenarios.FindAsync(id);
            if (scenario == null)
            {
                return NotFound();
            }

            var scenarioObjectives = await _context.ScenarioObjectives
                .Where(so => so.ScenarioId == id)
                .ToListAsync();

            var viewModels = scenarioObjectives.Select(so => new ScenarioObjectiveEditVm
            {
                Id = so.Id,
                ScenarioId = so.ScenarioId,
                HexId = so.HexId,
                SideId = so.SideId,
                VictoryPoints = so.VictoryPoints,
                ConditionKind = (int)so.ConditionKind,
                TurnThreshold = so.TurnThreshold,
                Hexes = _selectListService.GetHexesAsync().Result,
                Sides = _selectListService.GetSidesAsync().Result
            }).ToList();

            ViewBag.ScenarioId = id;
            ViewBag.ScenarioName = scenario.Name;
            return View(viewModels);
        }

        // POST: Scenarios/AddObjective
        [HttpPost]
        public async Task<IActionResult> AddObjective(int scenarioId)
        {
            var viewModel = new ScenarioObjectiveEditVm
            {
                ScenarioId = scenarioId,
                Hexes = await _selectListService.GetHexesAsync(),
                Sides = await _selectListService.GetSidesAsync()
            };

            return PartialView("_ObjectiveRow", viewModel);
        }

        // POST: Scenarios/SaveObjective
        [HttpPost]
        public async Task<IActionResult> SaveObjective([FromBody]ScenarioObjectiveEditVm viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var scenarioObjective = new ScenarioObjective
                    {
                        ScenarioId = viewModel.ScenarioId,
                        HexId = viewModel.HexId,
                        SideId = viewModel.SideId,
                        VictoryPoints = viewModel.VictoryPoints,
                        ConditionKind = (VictoryConditionKind)viewModel.ConditionKind,
                        TurnThreshold = viewModel.TurnThreshold
                    };

                    _context.Add(scenarioObjective);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                viewModel.Hexes = await _selectListService.GetHexesAsync();
                viewModel.Sides = await _selectListService.GetSidesAsync();
                return PartialView("_ObjectiveRow", viewModel);
            }
            catch (Exception ex)
            {

                throw;
            }

           
        }

        // POST: Scenarios/DeleteObjective
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteObjective(int id)
        {
            var scenarioObjective = await _context.ScenarioObjectives.FindAsync(id);
            if (scenarioObjective != null)
            {
                _context.ScenarioObjectives.Remove(scenarioObjective);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Objective not found" });
        }

        private bool ScenarioExists(int id)
        {
            return _context.Scenarios.Any(e => e.Id == id);
        }
    }
}
