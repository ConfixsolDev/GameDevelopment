using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;
using System.Text;

namespace TechWebSol.Controllers
{
    [Authorize]
    public class WarGameArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<WarGameArchiveController> _logger;
        private readonly ApplicationUserVM _user;
        private readonly IReportRenderService _reportRenderer;
        private readonly IPdfGeneratorService _pdfGenerator;

        public WarGameArchiveController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<WarGameArchiveController> logger,
            IReportRenderService reportRenderer,
            IPdfGeneratorService pdfGenerator)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
            _user = userSessionService.GetCurrentUser();
            _reportRenderer = reportRenderer;
            _pdfGenerator = pdfGenerator;
        }

        /// <summary>
        /// Archive viewer - displays archived games
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Title"] = "War Game Archive";
            ViewData["Subtitle"] = "Browse Historical War Games";
            return View();
        }

        /// <summary>
        /// View a specific archived game (read-only)
        /// </summary>
        public async Task<IActionResult> ViewArchive(Guid id)
        {
            try
            {
                var archive = await _context.WarGameArchives
                    .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

                if (archive == null)
                {
                    return NotFound();
                }

                ViewData["Title"] = archive.GameTitle;
                ViewData["Subtitle"] = $"Archived Game - {archive.GameMonth} {archive.GameYear}";
                ViewData["ArchiveId"] = archive.Id;
                ViewData["ArchiveData"] = new
                {
                    gameTitle = archive.GameTitle,
                    gameCode = archive.GameCode,
                    gameMonth = archive.GameMonth,
                    gameYear = archive.GameYear,
                    currentTurn = archive.CurrentTurn,
                    totalTurns = archive.TotalTurns,
                    gameStateJson = archive.GameStateJson,
                    mapOverlaysJson = archive.MapOverlaysJson,
                    forcesJson = archive.ForcesJson,
                    attacksJson = archive.AttacksJson,
                    maneuversJson = archive.ManeuversJson,
                    defenseElementsJson = archive.DefenseElementsJson,
                    gameTurnsJson = archive.GameTurnsJson,
                    adjudicationResultsJson = archive.AdjudicationResultsJson,
                    combatResultsJson = archive.CombatResultsJson,
                    savedDate = archive.SavedDate
                };

                return View("ViewArchive");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading archive {ArchiveId}", id);
                return StatusCode(500, "Error loading archive");
            }
        }

        /// <summary>
        /// Save the last game played into the archive
        /// Can be called by Director to save current game state
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveLastGame([FromBody] SaveGameRequest request)
        {
            try
            {
                if (_user == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Check if user is Director (ApplicationRole == true)
                if (!_user.ApplicationRole)
                {
                    return StatusCode(403, new { success = false, message = "Only Directors can save games" });
                }

                _logger.LogInformation("Saving last game by user {UserId}", _user.ApplicationUserId);

                // Collect all game data
                var gameData = await CollectGameDataAsync(request.TurnNumber ?? 1);

                // Create archive entry
                var archive = new WarGameArchive
                {
                    Id = Guid.NewGuid(),
                    GameTitle = request.GameTitle ?? $"War Game {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                    Description = request.Description,
                    GameCode = request.GameCode ?? $"WG{DateTime.UtcNow:yyyyMMddHHmmss}",
                    GameMonth = DateTime.UtcNow.ToString("MMMM"),
                    GameYear = DateTime.UtcNow.Year,
                    CurrentTurn = request.TurnNumber ?? 1,
                    TotalTurns = request.TotalTurns ?? 1,
                    GameSessionId = request.GameSessionId,
                    MapConfigurationId = request.MapConfigurationId,
                    GameStateJson = JsonConvert.SerializeObject(gameData.GameState),
                    MapOverlaysJson = JsonConvert.SerializeObject(gameData.MapOverlays),
                    ForcesJson = JsonConvert.SerializeObject(gameData.Forces),
                    AttacksJson = JsonConvert.SerializeObject(gameData.Attacks),
                    ManeuversJson = JsonConvert.SerializeObject(gameData.Maneuvers),
                    DefenseElementsJson = JsonConvert.SerializeObject(gameData.DefenseElements),
                    GameTurnsJson = gameData.GameTurns,
                    AdjudicationResultsJson = JsonConvert.SerializeObject(gameData.AdjudicationResults),
                    CombatResultsJson = JsonConvert.SerializeObject(gameData.CombatResults),
                    Status = "Archived",
                    SavedByUserId = _user.ApplicationUserId,
                    SavedByUserName = _user.FullName,
                    SavedDate = DateTime.UtcNow,
                    CreatedBy = _user.ApplicationUserId,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                // If this is a turn save, update or append to existing archive
                if (request.IsTurnSave && request.ArchiveId.HasValue)
                {
                    var existingArchive = await _context.WarGameArchives
                        .FirstOrDefaultAsync(a => a.Id == request.ArchiveId.Value);

                    if (existingArchive != null)
                    {
                        // Update existing archive and append turn data
                        var existingTurns = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                            existingArchive.GameTurnsJson ?? "{}") ?? new Dictionary<string, object>();

                        var turnKey = $"turn{request.TurnNumber}";
                        existingTurns[turnKey] = gameData.GameState;

                        existingArchive.GameTurnsJson = JsonConvert.SerializeObject(existingTurns);
                        existingArchive.CurrentTurn = request.TurnNumber ?? existingArchive.CurrentTurn;
                        existingArchive.TotalTurns = Math.Max(existingArchive.TotalTurns, request.TurnNumber ?? existingArchive.TotalTurns);
                        existingArchive.UpdatedBy = _user.ApplicationUserId;
                        existingArchive.UpdatedDate = DateTime.UtcNow;

                        _context.WarGameArchives.Update(existingArchive);
                        await _context.SaveChangesAsync();

                        // Generate report document for this turn
                        var reportPath = await GenerateAndSaveReportDocument(existingArchive);
                        existingArchive.PdfDocumentPath = reportPath;
                        _context.WarGameArchives.Update(existingArchive);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Updated archive {ArchiveId} with turn {TurnNumber}", 
                            request.ArchiveId, request.TurnNumber);

                        return Json(new { success = true, message = "Turn saved successfully", archiveId = existingArchive.Id });
                    }
                }

                // Save new archive
                _context.WarGameArchives.Add(archive);
                await _context.SaveChangesAsync();

                // Generate report document for this archive
                var newReportPath = await GenerateAndSaveReportDocument(archive);
                archive.PdfDocumentPath = newReportPath;
                _context.WarGameArchives.Update(archive);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved new archive {ArchiveId} for game {GameTitle}", 
                    archive.Id, archive.GameTitle);

                return Json(new { success = true, message = "Game saved successfully", archiveId = archive.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game");
                return StatusCode(500, new { success = false, message = "Error saving game: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReport(Guid id)
        {
            var archive = await _context.WarGameArchives.FirstOrDefaultAsync(a => a.Id == id);
            if (archive == null) return NotFound();

            // Generate fresh if missing
            if (string.IsNullOrWhiteSpace(archive.PdfDocumentPath) || !System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", archive.PdfDocumentPath.TrimStart('/', '\\'))))
            {
                var path = await GenerateAndSaveReportDocument(archive);
                archive.PdfDocumentPath = path;
                _context.Update(archive);
                await _context.SaveChangesAsync();
            }

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(wwwroot, archive.PdfDocumentPath.TrimStart('/', '\\'));
            var fileName = Path.GetFileName(fullPath);
            var contentType = "application/pdf";
            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, contentType, fileName);
        }

        private async Task<string> GenerateAndSaveReportDocument(WarGameArchive archive)
        {
            // Prepare model for report view
            var model = new
            {
                archive.GameTitle,
                archive.GameCode,
                archive.GameMonth,
                archive.GameYear,
                archive.CurrentTurn,
                archive.TotalTurns,
                forcesJson = archive.ForcesJson,
                attacksJson = archive.AttacksJson,
                defenseElementsJson = archive.DefenseElementsJson,
                mapOverlaysJson = archive.MapOverlaysJson,
                gameStateJson = archive.GameStateJson,
                adjudicationResultsJson = archive.AdjudicationResultsJson,
                combatResultsJson = archive.CombatResultsJson
            };

            // Render Razor view to HTML string
            var html = await _reportRenderer.RenderViewToStringAsync(this.ControllerContext, 
                "Views/WarGameArchive/Reports/WarGameReport.cshtml", model);

            // Convert HTML to PDF
            var pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(html);

            // Save PDF to wwwroot/reports
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var reportsDir = Path.Combine(wwwroot, "reports");
            Directory.CreateDirectory(reportsDir);
            var fileName = $"wargame-report-{archive.Id}-turn-{archive.CurrentTurn}.pdf";
            var fullPath = Path.Combine(reportsDir, fileName);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            // Return relative path to serve
            var relativePath = $"/reports/{fileName}";
            return relativePath;
        }

        /// <summary>
        /// Get list of all archived games
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetArchives()
        {
            try
            {
                var archives = await _context.WarGameArchives
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.SavedDate ?? a.CreatedDate)
                    .Select(a => new
                    {
                        id = a.Id,
                        gameTitle = a.GameTitle,
                        gameCode = a.GameCode,
                        gameMonth = a.GameMonth,
                        gameYear = a.GameYear,
                        currentTurn = a.CurrentTurn,
                        totalTurns = a.TotalTurns,
                        status = a.Status,
                        savedBy = a.SavedByUserName,
                        savedDate = a.SavedDate ?? a.CreatedDate
                    })
                    .ToListAsync();

                return Json(new { success = true, archives });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading archives");
                return StatusCode(500, new { success = false, message = "Error loading archives" });
            }
        }

        /// <summary>
        /// Collect all game data for archiving
        /// </summary>
        private async Task<GameDataCollection> CollectGameDataAsync(int turnNumber)
        {
            var gameData = new GameDataCollection();

            try
            {
                // 1. Collect all tokens with positions
                var tokens = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .Include(t => t.MapMarkers.OrderBy(m => m.CreatedDate))
                    .Where(t => t.IsActive)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        forceType = t.ForceType,
                        tokenGroup = t.TokenGroup != null ? t.TokenGroup.Name : null,
                        teamId = t.TeamId,
                        markers = t.MapMarkers.Select(m => new
                        {
                            id = m.Id,
                            latitude = m.latitude,
                            longitude = m.longitude,
                            createdDate = m.CreatedDate,
                            isActive = m.IsActive
                        }).ToList()
                    })
                    .ToListAsync();

                gameData.Forces = tokens;

                // 2. Collect all attack orders
                var attackOrders = await _context.AttackOrders
                    .Include(a => a.AttackerToken)
                    .Include(a => a.TargetToken)
                    .Where(a => a.IsActive)
                    .Select(a => new
                    {
                        id = a.Id,
                        attackerTokenId = a.AttackerTokenId,
                        targetTokenId = a.TargetTokenId,
                        attackerTokenName = a.AttackerToken.Name,
                        targetTokenName = a.TargetToken.Name,
                        posture = a.Posture,
                        status = a.Status,
                        expectedStartTurn = a.ExpectedStartTurn,
                        durationTurns = a.DurationTurns,
                        payloadJson = a.PayloadJson,
                        createdDate = a.CreatedDate
                    })
                    .ToListAsync();

                var enhancedAttackOrders = await _context.EnhancedAttackOrders
                    .Where(a => a.IsActive)
                    .Select(a => new
                    {
                        id = a.Id,
                        attackerTokenId = a.AttackerTokenId,
                        targetTokenId = a.TargetTokenId,
                        intentJson = a.IntentJson,
                        firesJson = a.FiresJson,
                        movementJson = a.MovementJson,
                        timingJson = a.TimingJson,
                        status = a.Status,
                        createdDate = a.CreatedDate
                    })
                    .ToListAsync();

                gameData.Attacks = new
                {
                    attackOrders,
                    enhancedAttackOrders
                };

                // 3. Collect all defense elements
                var defenseElements = await _context.DefenseElements
                    .Include(d => d.Token)
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        id = d.Id,
                        elementId = d.ElementId,
                        category = d.Category,
                        type = d.Type,
                        coordinates = d.Coordinates,
                        tokenId = d.TokenId,
                        tokenName = d.Token != null ? d.Token.Name : null,
                        teamId = d.TeamId,
                        strength = d.Strength,
                        effectiveness = d.Effectiveness,
                        visibility = d.Visibility,
                        status = d.Status,
                        metadata = d.Metadata
                    })
                    .ToListAsync();

                gameData.DefenseElements = defenseElements;

                // 4. Collect map overlays
                var mapRegions = await _context.MapRegions
                    .Where(r => r.IsActive)
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        geometry = r.Geometry,
                        properties = r.Properties,
                        regionType = r.RegionType
                    })
                    .ToListAsync();

                var mapSectors = await _context.MapSectors
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        geometry = s.Geometry,
                        landType = s.LandType,
                        properties = s.Properties
                    })
                    .ToListAsync();

                var mapLabels = await _context.MapLabels
                    .Where(l => l.IsActive)
                    .Select(l => new
                    {
                        id = l.Id,
                        text = l.Text,
                        latitude = l.Latitude,
                        longitude = l.Longitude,
                        labelType = l.LabelType
                    })
                    .ToListAsync();

                gameData.MapOverlays = new
                {
                    regions = mapRegions,
                    sectors = mapSectors,
                    labels = mapLabels
                };

                // 5. Collect map markers (movements/maneuvers)
                var mapMarkers = await _context.MapMarkers
                    .Include(m => m.Token)
                    .Where(m => m.IsActive)
                    .Select(m => new
                    {
                        id = m.Id,
                        tokenId = m.TokenId,
                        tokenName = m.Token != null ? m.Token.Name : null,
                        latitude = m.latitude,
                        longitude = m.longitude,
                        createdDate = m.CreatedDate
                    })
                    .ToListAsync();

                gameData.Maneuvers = mapMarkers;

                // 6. Create game state snapshot
                gameData.GameState = new
                {
                    turnNumber = turnNumber,
                    timestamp = DateTime.UtcNow,
                    tokenCount = tokens.Count,
                    attackOrderCount = attackOrders.Count,
                    defenseElementCount = defenseElements.Count
                };

                // 7. Initialize empty game turns JSON
                gameData.GameTurns = "{}";

                // 8. Initialize empty adjudication and combat results
                gameData.AdjudicationResults = new { };
                gameData.CombatResults = new { };

                return gameData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting game data");
                throw;
            }
        }
    }

    /// <summary>
    /// Request model for saving a game
    /// </summary>
    public class SaveGameRequest
    {
        public string? GameTitle { get; set; }
        public string? Description { get; set; }
        public string? GameCode { get; set; }
        public int? TurnNumber { get; set; }
        public int? TotalTurns { get; set; }
        public Guid? GameSessionId { get; set; }
        public Guid? MapConfigurationId { get; set; }
        public Guid? ArchiveId { get; set; }
        public bool IsTurnSave { get; set; } = false;
    }

    /// <summary>
    /// Game data collection helper class
    /// </summary>
    internal class GameDataCollection
    {
        public object GameState { get; set; } = new { };
        public object MapOverlays { get; set; } = new { };
        public object Forces { get; set; } = new { };
        public object Attacks { get; set; } = new { };
        public object Maneuvers { get; set; } = new { };
        public object DefenseElements { get; set; } = new { };
        public string GameTurns { get; set; } = "{}";
        public object AdjudicationResults { get; set; } = new { };
        public object CombatResults { get; set; } = new { };
    }
}

