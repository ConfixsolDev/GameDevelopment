using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Core.Data;
using WargameBoard.Web.Services;
using WargameBoard.Web.Models.ViewModels;

namespace WargameBoard.Web.Controllers
{
    public class GameSessionsController : Controller
    {
        private readonly WargameDbContext _db;
        private readonly GameSessionService _svc;
        private readonly ISelectListService _selectListService;
        
        public GameSessionsController(WargameDbContext db, GameSessionService svc, ISelectListService selectListService) 
        { 
            _db = db; 
            _svc = svc; 
            _selectListService = selectListService;
        }

        // GET: GameSessions
        public async Task<IActionResult> Index()
        {
            var sessions = await _db.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.CurrentSide)
                .Include(s => s.Turns)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            
            return View(sessions);
        }

        // GET: GameSessions/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SessionCreateVm
            {
                Scenarios = await _selectListService.GetScenariosAsync()
            };
            return View(viewModel);
        }

        // POST: GameSessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionCreateVm model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var session = await _svc.StartSessionAsync(model.ScenarioId, null, model.Notes);
                    TempData["SuccessMessage"] = "Session started successfully!";
                    return RedirectToAction(nameof(Details), new { id = session.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error starting session: {ex.Message}");
                }
            }

            model.Scenarios = await _selectListService.GetScenariosAsync();
            return View(model);
        }

        // GET: /Session/CreateFromScenario?scenarioId=1&sideId=2
        public async Task<IActionResult> CreateFromScenario(int scenarioId, int? sideId)
        {
            var session = await _svc.StartSessionAsync(scenarioId, sideId);
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }

        // GET: /Session/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var session = await _db.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.Turns)
                .Include(s => s.CurrentSide)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.TokenPiece)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.Hex)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (session == null) return NotFound();
            return View(session);
        }

        // GET: GameSessions/Join/5
        public async Task<IActionResult> Join(int id)
        {
            var session = await _db.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.CurrentSide)
                .Include(s => s.Scenario.ScenarioUnits)
                    .ThenInclude(su => su.Unit)
                .Include(s => s.Scenario.ScenarioUnits)
                    .ThenInclude(su => su.StartHex)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (session == null) return NotFound();
            
            ViewBag.Sides = await _selectListService.GetSidesAsync();
            return View(session);
        }

        // POST: GameSessions/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int sessionId, string playerName, int sideId)
        {
            var session = await _db.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.CurrentSide)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            
            if (session == null) return NotFound();
            
            // For now, just redirect to the game board
            // In a real implementation, you would store player assignments
            TempData["PlayerName"] = playerName;
            TempData["SideId"] = sideId;
            
            return RedirectToAction(nameof(RealTimeDetails), new { id = sessionId });
        }

        // POST: /Session/AdvanceTurn/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceTurn(int id)
        {
            await _svc.AdvanceTurnAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Session/End/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> End(int id)
        {
            await _svc.EndSessionAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Session/RealTimeDetails/5
        public async Task<IActionResult> RealTimeDetails(int id)
        {
            var session = await _db.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.Turns)
                .Include(s => s.CurrentSide)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (session == null) return NotFound();
            return View("RealTimeDetails", session);
        }
    }
}
