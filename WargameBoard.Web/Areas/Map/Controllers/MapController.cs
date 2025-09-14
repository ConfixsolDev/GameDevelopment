using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using WargameBoard.Web.Models.ViewModels;
using WargameBoard.Web.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using WargameBoard.Core.Data;
using Microsoft.AspNetCore.SignalR;
using WargameBoard.Web.Hubs;

namespace WargameBoard.Web.Areas.Map.Controllers
{
    [Area("Map")]
    public class MapController : Controller
    {
        private readonly WargameDbContext _context;
        private readonly ISelectListService _selectListService;
        private readonly IMapper _mapper;
        private readonly IRealTimeGameService _realTimeGameService;
        private readonly IHubContext<RealTimeGameHub> _hubContext;

        public MapController(
            WargameDbContext context, 
            ISelectListService selectListService, 
            IMapper mapper,
            IRealTimeGameService realTimeGameService,
            IHubContext<RealTimeGameHub> hubContext)
        {
            _context = context;
            _selectListService = selectListService;
            _mapper = mapper;
            _realTimeGameService = realTimeGameService;
            _hubContext = hubContext;
        }

        // GET: Map/Map
        public async Task<IActionResult> Index(int? sessionId)
        {
            // Load hexes for 12x12 grid
            var hexes = await _context.Hexes
                .Include(h => h.HexFeatures)
                    .ThenInclude(hf => hf.FortificationType)
                .Include(h => h.HexFeatures)
                    .ThenInclude(hf => hf.ObstacleType)
                .Include(h => h.HexFeatures)
                    .ThenInclude(hf => hf.Side)
                .Include(h => h.TerrainType)
                .Include(h => h.Placements)
                    .ThenInclude(p => p.TokenPiece)
                .ToListAsync();

            if (!hexes.Any())
            {
                var terrainIds = await _context.TerrainTypes.Select(t => t.Id).ToListAsync();
                var rnd = new Random(42);
                for (int q = 0; q < 12; q++)
                {
                    for (int r = 0; r < 12; r++)
                    {
                        _context.Hexes.Add(new Hex
                        {
                            Q = q,
                            R = r,
                            TerrainTypeId = terrainIds[rnd.Next(terrainIds.Count)],
                            KeyFeature = (q == 6 && r == 6) ? "Crossroads" : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                hexes = await _context.Hexes
                    .Include(h => h.TerrainType)
                    .Include(h => h.HexFeatures)
                    .Include(h => h.Placements)
                        .ThenInclude(p => p.TokenPiece)
                    .ToListAsync();
            }

            ViewBag.SessionId = sessionId;
            ViewBag.TerrainTypes = await _selectListService.GetTerrainTypesAsync();
            ViewBag.FortificationTypes = await _selectListService.GetFortificationTypesAsync();
            ViewBag.ObstacleTypes = await _selectListService.GetObstacleTypesAsync();
            ViewBag.Sides = await _selectListService.GetSidesAsync();
            ViewBag.TokenPieces = await _selectListService.GetTokenPiecesAsync();
            ViewBag.Sessions = await _selectListService.GetSessionsAsync();

            return View(hexes);
        }

        // GET: Map/Map/EditHex/5
        public async Task<IActionResult> EditHex(int id)
        {
            var hex = await _context.Hexes.FindAsync(id);
            if (hex == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<HexEditVm>(hex);
            await PopulateSelectListsAsync(viewModel);

            return PartialView("_EditHexModal", viewModel);
        }

        // POST: Map/Map/EditHex/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHex(int id, HexEditVm model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hex = _mapper.Map<Hex>(model);
                    _context.Update(hex);
                    await _context.SaveChangesAsync();
                    
                    // Return success message
                    return Content("<div class='alert alert-success'>Hex updated successfully.</div>");
                }
                catch (Exception ex)
                {
                    if (!HexExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateSelectListsAsync(model);
            return PartialView("_EditHexModal", model);
        }

        // GET: Map/Map/ListHexFeatures/5
        public async Task<IActionResult> ListHexFeatures(int hexId)
        {
            var features = await _context.HexFeatures
                .Where(hf => hf.HexId == hexId)
                .Include(hf => hf.FortificationType)
                .Include(hf => hf.ObstacleType)
                .Include(hf => hf.Side)
                .ToListAsync();

            ViewBag.HexId = hexId;
            return PartialView("_ListHexFeatures", features);
        }

        // GET: Map/Map/AddFeature
        public async Task<IActionResult> AddFeature(int hexId)
        {
            var model = new HexFeatureEditVm
            {
                HexId = hexId
            };

            await PopulateSelectListsAsync(model);
            return PartialView("_AddFeature", model);
        }

        // POST: Map/Map/AddFeature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeature(HexFeatureEditVm model)
        {
            if (ModelState.IsValid)
            {
                var feature = _mapper.Map<HexFeature>(model);
                _context.Add(feature);
                await _context.SaveChangesAsync();
                
                // Return success message and refresh features list
                var features = await _context.HexFeatures
                    .Where(hf => hf.HexId == model.HexId)
                    .Include(hf => hf.FortificationType)
                    .Include(hf => hf.ObstacleType)
                    .Include(hf => hf.Side)
                    .ToListAsync();

                ViewBag.HexId = model.HexId;
                return PartialView("_ListHexFeatures", features);
            }

            await PopulateSelectListsAsync(model);
            return PartialView("_AddFeature", model);
        }

        // POST: Map/Map/DeleteFeature/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeature(int id)
        {
            var feature = await _context.HexFeatures.FindAsync(id);
            if (feature != null)
            {
                _context.HexFeatures.Remove(feature);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Feature deleted successfully." });
            }

            return Json(new { success = false, message = "Feature not found." });
        }

        private bool HexExists(int id)
        {
            return _context.Hexes.Any(e => e.Id == id);
        }

        private async Task PopulateSelectListsAsync(HexEditVm model)
        {
            model.TerrainTypes = await _selectListService.GetTerrainTypesAsync();
        }

        private async Task PopulateSelectListsAsync(HexFeatureEditVm model)
        {
            model.FortificationTypes = await _selectListService.GetFortificationTypesAsync();
            model.ObstacleTypes = await _selectListService.GetObstacleTypesAsync();
            model.Sides = await _selectListService.GetSidesAsync();
        }

        // Real-time API endpoints
        [HttpPost]
        public async Task<IActionResult> PlaceToken(int sessionId, int tokenPieceId, int hexId)
        {
            var success = await _realTimeGameService.PlaceTokenAsync(sessionId, tokenPieceId, hexId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> MoveToken(int sessionId, int placementId, int newHexId)
        {
            var success = await _realTimeGameService.MoveTokenAsync(sessionId, placementId, newHexId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveToken(int sessionId, int placementId)
        {
            var success = await _realTimeGameService.RemoveTokenAsync(sessionId, placementId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHexTerrain(int sessionId, int hexId, int terrainTypeId)
        {
            var success = await _realTimeGameService.UpdateHexTerrainAsync(sessionId, hexId, terrainTypeId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> AddHexFeature(int sessionId, int hexId, int featureTypeId, string featureKind, int? sideId)
        {
            var success = await _realTimeGameService.AddHexFeatureAsync(sessionId, hexId, featureTypeId, featureKind, sideId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveHexFeature(int sessionId, int featureId)
        {
            var success = await _realTimeGameService.RemoveHexFeatureAsync(sessionId, featureId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceTurn(int sessionId)
        {
            var success = await _realTimeGameService.AdvanceTurnAsync(sessionId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateObjectiveControl(int sessionId, int objectiveId, int sideId, string action)
        {
            var success = await _realTimeGameService.UpdateObjectiveControlAsync(sessionId, objectiveId, sideId, action);
            return Json(new { success });
        }

        // Get current session state
        [HttpGet]
        public async Task<IActionResult> GetSessionState(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.Turns)
                .Include(s => s.CurrentSide)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.TokenPiece)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.Hex)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var sessionData = new
            {
                Id = session.Id,
                Scenario = session.Scenario?.Name,
                CurrentSide = session.CurrentSide?.Name,
                Turns = session.Turns.Select(t => new
                {
                    Number = t.Number,
                    StartedAt = t.StartedAt,
                    EndedAt = t.EndedAt
                }),
                Placements = session.Placements.Select(p => new
                {
                    Id = p.Id,
                    TokenPieceId = p.TokenPieceId,
                    HexId = p.HexId,
                    Q = p.Hex?.Q,
                    R = p.Hex?.R,
                    PlacedAt = p.PlacedAt
                })
            };

            return Json(sessionData);
        }
    }
}
