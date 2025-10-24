using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    [Authorize]
    public class DataManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DataManagementController> _logger;
        private readonly ApplicationUserVM user;

        public DataManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserSessionService userSessionService,
            ILogger<DataManagementController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            user = userSessionService.GetCurrentUser();
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Token-Based Data Management";
            ViewData["Subtitle"] = "Military Unit Data Entry System";
            ViewBag.TeamId = user?.TeamId ?? Guid.Empty;
            return View();
        }

        #region Token Data Entry Forms

        /// <summary>
        /// Returns the data entry form modal for a specific token
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TokenDataEntryForm(Guid tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                // Check if token already has brigade data
                var existingBrigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive);

                var viewModel = new TokenDataEntryViewModel
                {
                    Token = token,
                    ExistingBrigade = existingBrigade,
                    AvailableBrigades = await _context.Brigades
                        .Where(b => b.TeamId == user.TeamId && b.IsActive && b.TokenId == null)
                        .OrderBy(b => b.BrigadeCode)
                        .ToListAsync(),
                };

                // Load ALL units for this token (both with and without brigade)
                viewModel.ExistingInfantry = await _context.InfantryBattalions
                    .Where(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive)
                    .OrderByDescending(b => b.CreatedDate)
                    .ToListAsync();

                viewModel.ExistingArmoured = await _context.ArmouredRegiments
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                viewModel.ExistingArtillery = await _context.ArtilleryRegiments
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                // Load Logistics Units
                viewModel.ExistingLogistics = await _context.LogisticsUnits
                    .FirstOrDefaultAsync(l => l.TokenId == tokenId && l.TeamId == user.TeamId && l.IsActive);

                // Load Combat Engineering Companies
                viewModel.ExistingEngineering = await _context.CombatEngineeringCompanies
                    .FirstOrDefaultAsync(c => c.TokenId == tokenId && c.TeamId == user.TeamId && c.IsActive);

                // Load Reconnaissance (not tied to brigade)
                viewModel.ExistingRecon = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();


                return PartialView("Partials/_TokenDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading data entry form" });
            }
        }

        /// <summary>
        /// Returns the direct unit form for adding units directly to token (without brigade)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DirectUnitForm(UnitType unitType, Guid tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found" });
                }

                var viewModel = new DirectUnitFormViewModel
                {
                    UnitType = unitType,
                    Token = token,
                    TeamId = user.TeamId,
                    ForceType = token.ForceType,
                };

                // Load existing units for this token (without brigade filter)
                switch (unitType)
                {
                    case UnitType.Infantry:
                        viewModel.ExistingInfantry = await _context.InfantryBattalions
                            .Where(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive)
                            .OrderByDescending(b => b.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Armoured:
                        viewModel.ExistingArmoured = await _context.ArmouredRegiments
                            .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Artillery:
                        viewModel.ExistingArtillery = await _context.ArtilleryRegiments
                            .Where(a => a.TokenId == tokenId && a.TeamId == user.TeamId && a.IsActive)
                            .OrderByDescending(a => a.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Recon:
                        viewModel.ExistingRecon = await _context.Recon
                            .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Logistics:
                        viewModel.ExistingLogistics = await _context.LogisticsUnits
                            .FirstOrDefaultAsync(l => l.TokenId == tokenId && l.TeamId == user.TeamId && l.IsActive);
                        break;

                    case UnitType.Engineering:
                        viewModel.ExistingEngineering = await _context.CombatEngineeringCompanies
                            .FirstOrDefaultAsync(e => e.TokenId == tokenId && e.TeamId == user.TeamId && e.IsActive);
                        break;
                }

                return PartialView("Partials/_DirectUnitForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading direct unit form");
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading unit form" });
            }
        }

        /// <summary>
        /// Returns the single unit form for adding/editing units
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SingleUnitForm(UnitType unitType, Guid tokenId, Guid brigadeId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == brigadeId && b.TeamId == user.TeamId && b.IsActive);

                if (token == null || brigade == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token or Brigade not found" });
                }

                var viewModel = new SingleUnitFormViewModel
                {
                    UnitType = unitType,
                    Token = token,
                    Brigade = brigade,
                    TeamId = user.TeamId,
                    ForceType = brigade.ForceType,
                };

                switch (unitType)
                {
                    case UnitType.Infantry:
                        viewModel.ExistingInfantry = await _context.InfantryBattalions
                            .Where(b => b.TokenId == tokenId && b.BrigadeId == brigadeId && b.TeamId == user.TeamId && b.IsActive)
                            .OrderByDescending(b => b.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Armoured:
                        viewModel.ExistingArmoured = await _context.ArmouredRegiments
                            .Where(r => r.TokenId == tokenId && r.BrigadeId == brigadeId && r.TeamId == user.TeamId && r.IsActive)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Artillery:
                        viewModel.ExistingArtillery = await _context.ArtilleryRegiments
                            .Where(r => r.TokenId == tokenId && r.BrigadeId == brigadeId && r.TeamId == user.TeamId && r.IsActive)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToListAsync();
                        break;

                    case UnitType.Recon:
                        viewModel.ExistingRecon = await _context.Recon
                            .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToListAsync();
                        break;

                    default:
                        return PartialView("Partials/_ErrorPartial", new { Message = "Invalid unit type" });
                }

                return PartialView("Partials/_SingleUnitForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading unit form" });
            }
        }

        /// <summary>
        /// Returns the direct unit creation form (without brigade requirement)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DirectUnitCreationForm(string unitType, Guid tokenId, Guid? unitId = null)
        {
            try
            {
                _logger.LogInformation($"Loading direct unit creation form for unitType: {unitType}, tokenId: {tokenId}, unitId: {unitId}");

                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    _logger.LogWarning($"Token not found: {tokenId}");
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                // Parse unit type from string
                UnitType parsedUnitType;
                if (!Enum.TryParse(unitType, true, out parsedUnitType))
                {
                    _logger.LogWarning($"Invalid unit type: {unitType}");
                    return PartialView("Partials/_ErrorPartial", new { Message = "Invalid unit type" });
                }

                var viewModel = new DirectUnitCreationViewModel
                {
                    UnitType = parsedUnitType,
                    Token = token,
                    TeamId = user.TeamId,
                    ForceType = token.ForceType ?? "Unknown",
                    UnitId = unitId
                };

                // If editing, load existing unit data
                if (unitId.HasValue)
                {
                    viewModel.ExistingUnitJson = await GetExistingUnitJson(parsedUnitType, unitId.Value);
                }

                _logger.LogInformation($"Successfully created viewModel for {parsedUnitType} unit {(unitId.HasValue ? "edit" : "creation")}");

                return PartialView("Partials/_DirectUnitCreationForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading direct unit creation form");
                return PartialView("Partials/_ErrorPartial", new { Message = $"Error loading unit creation form: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get existing unit data as JSON for edit mode
        /// </summary>
        private async Task<string> GetExistingUnitJson(UnitType unitType, Guid unitId)
        {
            try
            {
                object unitData = null;

                switch (unitType)
                {
                    case UnitType.Infantry:
                        unitData = await _context.InfantryBattalions
                            .FirstOrDefaultAsync(u => u.Id == unitId && u.TeamId == user.TeamId && u.IsActive);
                        break;

                    case UnitType.Armoured:
                        unitData = await _context.ArmouredRegiments
                            .FirstOrDefaultAsync(u => u.Id == unitId && u.TeamId == user.TeamId && u.IsActive);
                        break;

                    case UnitType.Artillery:
                        unitData = await _context.ArtilleryRegiments
                            .FirstOrDefaultAsync(u => u.Id == unitId && u.TeamId == user.TeamId && u.IsActive);
                        break;

                    case UnitType.Logistics:
                        unitData = await _context.LogisticsUnits
                            .FirstOrDefaultAsync(u => u.Id == unitId && u.TeamId == user.TeamId && u.IsActive);
                        break;

                    case UnitType.Engineering:
                        unitData = await _context.CombatEngineeringCompanies
                            .FirstOrDefaultAsync(u => u.Id == unitId && u.TeamId == user.TeamId && u.IsActive);
                        break;
                }

                if (unitData != null)
                {
                    return System.Text.Json.JsonSerializer.Serialize(unitData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading existing unit data for {unitType} with ID {unitId}");
            }

            return null;
        }

        /// <summary>
        /// Returns the brigade unit creation/edit form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BrigadeUnitCreationForm(string unitType, Guid tokenId, Guid brigadeId, Guid? unitId = null)
        {
            try
            {
                _logger.LogInformation($"Loading brigade unit creation form for unitType: {unitType}, tokenId: {tokenId}, brigadeId: {brigadeId}, unitId: {unitId}");

                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    _logger.LogWarning($"Token not found: {tokenId}");
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == brigadeId && b.TeamId == user.TeamId && b.IsActive);

                if (brigade == null)
                {
                    _logger.LogWarning($"Brigade not found: {brigadeId}");
                    return PartialView("Partials/_ErrorPartial", new { Message = "Brigade not found or not accessible" });
                }

                // Parse unit type from string
                UnitType parsedUnitType;
                if (!Enum.TryParse(unitType, true, out parsedUnitType))
                {
                    _logger.LogWarning($"Invalid unit type: {unitType}");
                    return PartialView("Partials/_ErrorPartial", new { Message = "Invalid unit type" });
                }

                var viewModel = new BrigadeUnitCreationViewModel
                {
                    UnitType = parsedUnitType,
                    Token = token,
                    Brigade = brigade,
                    TeamId = user.TeamId,
                    ForceType = token.ForceType ?? "Unknown",
                    UnitId = unitId
                };

                // If editing, load existing unit data
                if (unitId.HasValue)
                {
                    viewModel.ExistingUnitJson = await GetExistingUnitJson(parsedUnitType, unitId.Value);
                }

                _logger.LogInformation($"Successfully created viewModel for brigade {parsedUnitType} unit {(unitId.HasValue ? "edit" : "creation")}");

                return PartialView("Partials/_BrigadeUnitCreationForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brigade unit creation form");
                return PartialView("Partials/_ErrorPartial", new { Message = $"Error loading unit creation form: {ex.Message}" });
            }
        }

        /// <summary>
        /// Returns the new brigade creation form for a specific token
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> NewBrigadeDataEntryForm(Guid tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                var viewModel = new NewBrigadeDataEntryViewModel
                {
                    Token = token,
                    Brigade = new Brigade 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = user.ForceType,  // Automatically from logged-in user
                        BrigadeCode = ""  // To be filled by user
                    },
                    InfantryBattalion = new InfantryBattalion 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = user.ForceType  // Automatically from logged-in user
                    },
                    ArmouredRegiment = new ArmouredRegiment 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = user.ForceType  // Automatically from logged-in user
                    },
                    ArtilleryRegiment = new ArtilleryRegiment 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = user.ForceType  // Automatically from logged-in user
                    }
                };

                return PartialView("Partials/_NewBrigadeDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading new brigade form" });
            }
        }

        /// <summary>
        /// Returns the units data entry form for a specific brigade (used after brigade creation)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UnitsDataEntryForm(Guid tokenId, Guid brigadeId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == brigadeId && b.TeamId == user.TeamId && b.IsActive);

                if (brigade == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Brigade not found or not accessible" });
                }

                // Get ALL existing units for this brigade (not just one)
                var existingInfantryList = await _context.InfantryBattalions
                    .Where(i => i.BrigadeId == brigadeId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var existingArmouredList = await _context.ArmouredRegiments
                    .Where(a => a.BrigadeId == brigadeId && a.IsActive)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var existingArtilleryList = await _context.ArtilleryRegiments
                    .Where(ar => ar.BrigadeId == brigadeId && ar.IsActive)
                    .OrderByDescending(ar => ar.CreatedDate)
                    .ToListAsync();

                // Recon is linked to Token, not Brigade
                var existingReconList = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                var existingLogisticsList = await _context.LogisticsUnits
                    .Where(l => l.BrigadeId == brigade.Id && l.TeamId == user.TeamId && l.IsActive)
                    .OrderByDescending(l => l.CreatedDate)
                    .ToListAsync();

                var existingEngineeringList = await _context.CombatEngineeringCompanies
                    .Where(e => e.BrigadeId == brigade.Id && e.TeamId == user.TeamId && e.IsActive)
                    .OrderByDescending(e => e.CreatedDate)
                    .ToListAsync();

                var viewModel = new UnitsDataEntryViewModel
                {
                    Token = token,
                    Brigade = brigade,
                    ExistingInfantryList = existingInfantryList,
                    ExistingArmouredList = existingArmouredList,
                    ExistingArtilleryList = existingArtilleryList,
                    ExistingReconList = existingReconList,
                    ExistingLogisticsList = existingLogisticsList,
                    ExistingEngineeringList = existingEngineeringList
                };

                return PartialView("Partials/_UnitsDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading units form" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenSummary(Guid tokenId)
        {
            try
            {
                // OPTIMIZED: Fetch all related data in parallel using Task.WhenAll
                // This is faster than sequential queries and doesn't require navigation properties
                var token = _context.Tokens.AsNoTracking().FirstOrDefault(t => t.Id == tokenId  && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", "Token not found or not accessible");
                }

                var Brigades = _context.Brigades
                    .AsNoTracking()
                    .Where(b => b.TokenId == tokenId  && b.IsActive)
                    .OrderByDescending(b => b.CreatedDate)
                    .FirstOrDefault();

                if (Brigades ==  null)
                {
                    Brigades = new Brigade();
                }

                // Load ALL units for this token (both with and without brigade)
                Brigades.InfantryBattalions = _context.InfantryBattalions
                    .Where(i => i.TokenId == tokenId  && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .AsNoTracking()
                    .ToList();

                Brigades.ArmouredRegiments = _context.ArmouredRegiments
                    .AsNoTracking()
                    .Where(a => a.TokenId == tokenId &&  a.IsActive)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToList();

                Brigades.ArtilleryRegiments = _context.ArtilleryRegiments
                    .AsNoTracking()
                    .Where(a => a.TokenId == tokenId  && a.IsActive)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToList();

                Brigades.LogisticsUnits = _context.LogisticsUnits
                    .AsNoTracking()
                    .Where(l => l.TokenId == tokenId  && l.IsActive)
                    .OrderByDescending(l => l.CreatedDate)
                    .ToList();

                Brigades.CombatEngineeringCompanies = _context.CombatEngineeringCompanies
                    .AsNoTracking()
                    .Where(e => e.TokenId == tokenId && e.IsActive)
                    .OrderByDescending(e => e.CreatedDate)
                    .ToList();

                // Load Recon separately (not part of Brigade)
                var reconList = _context.Recon
                    .AsNoTracking()
                    .Where(r => r.TokenId == tokenId  && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToList();

                // Create ViewModel
                var viewModel = new TokenSummaryViewModel
                {
                    Token = token,
                    Brigades = Brigades,
                    InfantryBattalions = Brigades.InfantryBattalions ?? new List<InfantryBattalion>(),
                    ArmouredRegiments = Brigades.ArmouredRegiments ?? new List<ArmouredRegiment>(),
                    ArtilleryRegiments = Brigades.ArtilleryRegiments ?? new List<ArtilleryRegiment>(),
                    LogisticsUnits = Brigades.LogisticsUnits ?? new List<LogisticsUnit>(),
                    CombatEngineeringCompanies = Brigades.CombatEngineeringCompanies ?? new List<CombatEngineeringCompany>(),
                    Recon = reconList
                };

                return PartialView("Partials/_TokenSummaryModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading token summary for token {TokenId}", tokenId);
                return PartialView("Partials/_ErrorPartial", new { Message = $"Error loading token summary: {ex.Message}" });
            }
        }

        #endregion

        #region Brigade List/Get Methods

        /// <summary>
        /// Get all brigades for the current team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBrigades()
        {
            try
            {
                var brigades = await _context.Brigades
                    .Where(b => b.TeamId == user.TeamId && b.IsActive)
                    .OrderBy(b => b.BrigadeCode)
                    .Select(b => new
                    {
                        id = b.Id,
                        brigadeCode = b.BrigadeCode,
                        forceType = b.ForceType,
                        tokenId = b.TokenId,
                        createdDate = b.CreatedDate
                    })
                    .ToListAsync();

                return Json(brigades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brigades");
                return Json(new { success = false, message = "Error loading brigades" });
            }
        }

        /// <summary>
        /// Get all infantry battalions for the current team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInfantryBattalions(Guid? brigadeId = null)
        {
            try
            {
                var query = _context.InfantryBattalions
                    .Include(i => i.Brigade)
                    .Where(i => i.TeamId == user.TeamId && i.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(i => i.BrigadeId == brigadeId.Value);
                }

                var battalions = await query
                    .OrderBy(i => i.Name)
                    .Select(i => new
                    {
                        id = i.Id,
                        name = i.Name,
                        unitCode = i.UnitCode,
                        strength = i.Strength,
                        companies = i.Companies,
                        forceType = i.ForceType,
                        description = i.Description,
                        atgms = i.ATGMS,
                        rocketLauncher = i.RocketLauncher,
                        mortars81mm = i.Mortars81mm,
                        mortars120mm = i.Mortars120mm,
                        grenadeLaunchers = i.GrenadeLaunchers,
                        hmg_AGL = i.HMG_AGL,
                        mg_LMG = i.MG_LMG,
                        manpads = i.MANPADS,
                        grenades = i.Grenades,
                        drones = i.Drones,
                        droneTypes = i.DroneTypes,
                        marchingSpeedTrucksRoads = i.MarchingSpeedTrucksRoads,
                        marchingSpeedAPCs = i.MarchingSpeedAPCs,
                        marchingSpeedCrossCountry = i.MarchingSpeedCrossCountry,
                        marchingSpeedAPCsCrossCountry = i.MarchingSpeedAPCsCrossCountry,
                        combatAdvanceSpeed = i.CombatAdvanceSpeed,
                        brigadeId = i.BrigadeId,
                        brigadeCode = i.Brigade != null ? i.Brigade.BrigadeCode : null,
                        tokenId = i.TokenId
                    })
                    .ToListAsync();

                return Json(battalions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading infantry battalions");
                return Json(new { success = false, message = "Error loading infantry battalions" });
            }
        }

        /// <summary>
        /// Get all armoured regiments for the current team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetArmouredRegiments(Guid? brigadeId = null)
        {
            try
            {
                var query = _context.ArmouredRegiments
                    .Include(a => a.Brigade)
                    .Where(a => a.TeamId == user.TeamId && a.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(a => a.BrigadeId == brigadeId.Value);
                }

                var regiments = await query
                    .OrderBy(a => a.Name)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        unitCode = a.UnitCode,
                        strength = a.Strength,
                        squadrons = a.Squadrons,
                        forceType = a.ForceType,
                        description = a.Description,
                        tanks = a.Tanks,
                        atgms = a.ATGMS,
                        mortars120mm = a.Mortars120mm,
                        hmg = a.HMG,
                        drones = a.Drones,
                        droneTypes = a.DroneTypes,
                        marchingSpeedRoads = a.MarchingSpeedRoads,
                        marchingSpeedCrossCountry = a.MarchingSpeedCrossCountry,
                        combatAdvanceSpeed = a.CombatAdvanceSpeed,
                        brigadeId = a.BrigadeId,
                        brigadeCode = a.Brigade != null ? a.Brigade.BrigadeCode : null,
                        tokenId = a.TokenId
                    })
                    .ToListAsync();

                return Json(regiments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading armoured regiments");
                return Json(new { success = false, message = "Error loading armoured regiments" });
            }
        }

        /// <summary>
        /// Get all artillery regiments for the current team
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetArtilleryRegiments(Guid? brigadeId = null)
        {
            try
            {
                var query = _context.ArtilleryRegiments
                    .Include(a => a.Brigade)
                    .Where(a => a.TeamId == user.TeamId && a.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(a => a.BrigadeId == brigadeId.Value);
                }

                var regiments = await query
                    .OrderBy(a => a.Name)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        unitCode = a.UnitCode,
                        strength = a.Strength,
                        batteries = a.Batteries,
                        forceType = a.ForceType,
                        description = a.Description,
                        guns = a.Guns,
                        gunRange = a.GunRange,
                        gunCaliber = a.GunCaliber,
                        hmg = a.HMG,
                        drones = a.Drones,
                        droneTypes = a.DroneTypes,
                        brigadeId = a.BrigadeId,
                        brigadeCode = a.Brigade != null ? a.Brigade.BrigadeCode : null,
                        tokenId = a.TokenId
                    })
                    .ToListAsync();

                return Json(regiments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading artillery regiments");
                return Json(new { success = false, message = "Error loading artillery regiments" });
            }
        }

        #endregion

        #region Brigade Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateTokenBrigade([FromBody] CreateTokenBrigadeRequest request)
        {
            try
            {
                var brigade = new Brigade
                {
                    BrigadeCode = request.BrigadeCode,
                    ForceType = user.ForceType,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    IsActive = true
                };

                _context.Brigades.Add(brigade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = brigade });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenBrigade([FromBody] Brigade brigade)
        {
            try
            {
                var existingBrigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == brigade.Id && b.TeamId == user.TeamId);

                if (existingBrigade == null) return NotFound();

                existingBrigade.BrigadeCode = brigade.BrigadeCode;
                existingBrigade.ForceType = brigade.ForceType;
                existingBrigade.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingBrigade });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenBrigade(Guid id)
        {
            try
            {
                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == id && b.TeamId == user.TeamId);

                if (brigade == null) return NotFound();

                brigade.IsActive = false;
                brigade.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LinkBrigadeToToken([FromBody] LinkBrigadeToTokenRequest request)
        {
            try
            {
                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == request.BrigadeId && b.TeamId == user.TeamId && b.IsActive);

                var existingBattalions = await _context.InfantryBattalions
                    .Where(b => b.BrigadeId == request.BrigadeId && b.TeamId == user.TeamId && b.IsActive)
                    .ToListAsync();

                var existingArmouredRegiments = await _context.ArmouredRegiments
                    .Where(r => r.BrigadeId == request.BrigadeId && r.TeamId == user.TeamId && r.IsActive)
                    .ToListAsync();

                var existingArtilleryRegiments = await _context.ArtilleryRegiments
                    .Where(r => r.BrigadeId == request.BrigadeId && r.TeamId == user.TeamId && r.IsActive)
                    .ToListAsync();

                if (brigade == null)
                {
                    return Json(new { success = false, message = "Brigade not found or not accessible." });
                }

                brigade.TokenId = request.TokenId;
                brigade.UpdatedBy = user.FullName;
                _context.Update(brigade);

                foreach (var battalion in existingBattalions)
                {
                    battalion.TokenId = request.TokenId;
                    battalion.UpdatedBy = user.FullName;
                    _context.Update(battalion);
                }

                foreach (var regiment in existingArmouredRegiments)
                {
                    regiment.TokenId = request.TokenId;
                    regiment.UpdatedBy = user.FullName;
                    _context.Update(regiment);
                }

                foreach (var regiment in existingArtilleryRegiments)
                {
                    regiment.TokenId = request.TokenId;
                    regiment.UpdatedBy = user.FullName;
                    _context.Update(regiment);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = brigade, message = "Brigade successfully linked to token." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion

        #region Infantry Battalion Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateTokenInfantryBattalion([FromBody] CreateTokenInfantryBattalionRequest request)
        {
            try
            {
                if (user == null) return Unauthorized();

                var battalion = new InfantryBattalion
                {
                    Id = request.Id ?? Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TokenId = request.TokenId,
                    TeamId = request.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true,
                    Companies = request.Companies,
                    ATGMS = request.ATGMS,
                    RocketLauncher = request.RocketLauncher,
                    Mortars81mm = request.Mortars81mm,
                    Mortars120mm = request.Mortars120mm,
                    GrenadeLaunchers = request.GrenadeLaunchers,
                    HMG_AGL = request.HMG_AGL,
                    MG_LMG = request.MG_LMG,
                    MANPADS = request.MANPADS,
                    Grenades = request.Grenades,
                    Drones = request.Drones,
                    DroneTypes = request.DroneTypes,
                    MarchingSpeedTrucksRoads = request.MarchingSpeedTrucksRoads,
                    MarchingSpeedAPCs = request.MarchingSpeedAPCs,
                    MarchingSpeedCrossCountry = request.MarchingSpeedCrossCountry,
                    MarchingSpeedAPCsCrossCountry = request.MarchingSpeedAPCsCrossCountry,
                    CombatAdvanceSpeed = request.CombatAdvanceSpeed
                };

                _context.InfantryBattalions.Add(battalion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = battalion });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenInfantryBattalion([FromBody] InfantryBattalion battalion)
        {
            try
            {
                var existingBattalion = await _context.InfantryBattalions
                    .FirstOrDefaultAsync(b => b.Id == battalion.Id && b.TeamId == user.TeamId);

                if (existingBattalion == null) return NotFound();

                existingBattalion.Name = battalion.Name;
                existingBattalion.Description = battalion.Description;
                existingBattalion.UnitCode = battalion.UnitCode;
                existingBattalion.Strength = battalion.Strength;
                existingBattalion.ForceType = battalion.ForceType;
                existingBattalion.Companies = battalion.Companies;
                existingBattalion.BrigadeId = battalion.BrigadeId;
                existingBattalion.TokenId = battalion.TokenId;
                existingBattalion.ATGMS = battalion.ATGMS;
                existingBattalion.RocketLauncher = battalion.RocketLauncher;
                existingBattalion.Mortars81mm = battalion.Mortars81mm;
                existingBattalion.Mortars120mm = battalion.Mortars120mm;
                existingBattalion.GrenadeLaunchers = battalion.GrenadeLaunchers;
                existingBattalion.HMG_AGL = battalion.HMG_AGL;
                existingBattalion.MG_LMG = battalion.MG_LMG;
                existingBattalion.MANPADS = battalion.MANPADS;
                existingBattalion.Grenades = battalion.Grenades;
                existingBattalion.Drones = battalion.Drones;
                existingBattalion.DroneTypes = battalion.DroneTypes;
                existingBattalion.MarchingSpeedTrucksRoads = battalion.MarchingSpeedTrucksRoads;
                existingBattalion.MarchingSpeedAPCs = battalion.MarchingSpeedAPCs;
                existingBattalion.MarchingSpeedCrossCountry = battalion.MarchingSpeedCrossCountry;
                existingBattalion.MarchingSpeedAPCsCrossCountry = battalion.MarchingSpeedAPCsCrossCountry;
                existingBattalion.CombatAdvanceSpeed = battalion.CombatAdvanceSpeed;
                existingBattalion.UpdatedBy = user.FullName;

                 _context.Update(existingBattalion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingBattalion });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenInfantryBattalion(Guid id)
        {
            try
            {
                var battalion = await _context.InfantryBattalions
                    .FirstOrDefaultAsync(b => b.Id == id && b.TeamId == user.TeamId);

                if (battalion == null) return NotFound();

                battalion.IsActive = false;
                battalion.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Armoured Regiment Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateTokenArmouredRegiment([FromBody] CreateTokenArmouredRegimentRequest request)
        {
            try
            {
                if (user == null) return Unauthorized();

                var regiment = new ArmouredRegiment
                {
                    Id = request.Id ?? Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TokenId = request.TokenId,
                    TeamId = request.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true,
                    Squadrons = request.Squadrons,
                    Tanks = request.Tanks,
                    ATGMS = request.ATGMS,
                    Mortars120mm = request.Mortars120mm,
                    HMG = request.HMG,
                    Drones = request.Drones,
                    DroneTypes = request.DroneTypes,
                    MarchingSpeedRoads = request.MarchingSpeedRoads,
                    MarchingSpeedCrossCountry = request.MarchingSpeedCrossCountry,
                    CombatAdvanceSpeed = request.CombatAdvanceSpeed
                };

                _context.ArmouredRegiments.Add(regiment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = regiment });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenArmouredRegiment([FromBody] ArmouredRegiment regiment)
        {
            try
            {
                var existingRegiment = await _context.ArmouredRegiments
                    .FirstOrDefaultAsync(r => r.Id == regiment.Id && r.TeamId == user.TeamId);

                if (existingRegiment == null) return NotFound();

                existingRegiment.Name = regiment.Name;
                existingRegiment.Description = regiment.Description;
                existingRegiment.UnitCode = regiment.UnitCode;
                existingRegiment.Strength = regiment.Strength;
                existingRegiment.ForceType = regiment.ForceType;
                existingRegiment.Squadrons = regiment.Squadrons;
                existingRegiment.BrigadeId = regiment.BrigadeId;
                existingRegiment.TokenId = regiment.TokenId;
                existingRegiment.Tanks = regiment.Tanks;
                existingRegiment.ATGMS = regiment.ATGMS;
                existingRegiment.Mortars120mm = regiment.Mortars120mm;
                existingRegiment.HMG = regiment.HMG;
                existingRegiment.Drones = regiment.Drones;
                existingRegiment.DroneTypes = regiment.DroneTypes;
                existingRegiment.MarchingSpeedRoads = regiment.MarchingSpeedRoads;
                existingRegiment.MarchingSpeedCrossCountry = regiment.MarchingSpeedCrossCountry;
                existingRegiment.CombatAdvanceSpeed = regiment.CombatAdvanceSpeed;
                existingRegiment.UpdatedBy = user.FullName;
                _context.Update(existingRegiment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingRegiment });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenArmouredRegiment(Guid id)
        {
            try
            {
                var regiment = await _context.ArmouredRegiments
                    .FirstOrDefaultAsync(r => r.Id == id && r.TeamId == user.TeamId);

                if (regiment == null) return NotFound();

                regiment.IsActive = false;
                regiment.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Artillery Regiment Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateTokenArtilleryRegiment([FromBody] CreateTokenArtilleryRegimentRequest request)
        {
            try
            {
                if (user == null) return Unauthorized();

                var regiment = new ArtilleryRegiment
                {
                    Id = request.Id ?? Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TokenId = request.TokenId,
                    TeamId = request.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true,
                    Batteries = request.Batteries,
                    Guns = request.Guns,
                    GunRange = request.GunRange,
                    GunCaliber = request.GunCaliber,
                    HMG = request.HMG,
                    Drones = request.Drones,
                    DroneTypes = request.DroneTypes
                };

                _context.ArtilleryRegiments.Add(regiment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = regiment });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenArtilleryRegiment([FromBody] ArtilleryRegiment regiment)
        {
            try
            {
                var existingRegiment = await _context.ArtilleryRegiments
                    .FirstOrDefaultAsync(r => r.Id == regiment.Id && r.TeamId == user.TeamId);

                if (existingRegiment == null) return NotFound();

                existingRegiment.Name = regiment.Name;
                existingRegiment.Description = regiment.Description;
                existingRegiment.UnitCode = regiment.UnitCode;
                existingRegiment.Strength = regiment.Strength;
                existingRegiment.ForceType = regiment.ForceType;
                existingRegiment.Batteries = regiment.Batteries;
                existingRegiment.BrigadeId = regiment.BrigadeId;
                existingRegiment.TokenId = regiment.TokenId;
                existingRegiment.Guns = regiment.Guns;
                existingRegiment.GunRange = regiment.GunRange;
                existingRegiment.GunCaliber = regiment.GunCaliber;
                existingRegiment.HMG = regiment.HMG;
                existingRegiment.Drones = regiment.Drones;
                existingRegiment.DroneTypes = regiment.DroneTypes;
                existingRegiment.UpdatedBy = user.FullName;
                _context.Update(existingRegiment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingRegiment });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenArtilleryRegiment(Guid id)
        {
            try
            {
                var regiment = await _context.ArtilleryRegiments
                    .FirstOrDefaultAsync(r => r.Id == id && r.TeamId == user.TeamId);

                if (regiment == null) return NotFound();

                regiment.IsActive = false;
                regiment.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Reconnaissance Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateTokenRecon([FromBody] CreateTokenReconRequest request)
        {
            try
            {
                var recon = new Recon
                {
                    Id = Guid.NewGuid(),
                    ReconType = request.ReconType,
                    Location = request.Location,
                    Confidence = request.Confidence,
                    Description = request.Description,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.Recon.Add(recon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = recon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenRecon([FromBody] Recon recon)
        {
            try
            {
                var existingRecon = await _context.Recon
                    .FirstOrDefaultAsync(r => r.Id == recon.Id && r.TeamId == user.TeamId);

                if (existingRecon == null) return NotFound();

                existingRecon.ReconType = recon.ReconType;
                existingRecon.Location = recon.Location;
                existingRecon.Confidence = recon.Confidence;
                existingRecon.Description = recon.Description;
                existingRecon.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingRecon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenRecon(Guid id)
        {
            try
            {
                var recon = await _context.Recon
                    .FirstOrDefaultAsync(r => r.Id == id && r.TeamId == user.TeamId);

                if (recon == null) return NotFound();

                recon.IsActive = false;
                recon.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Logistics Unit Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateLogisticsUnit([FromBody] CreateLogisticsUnitRequest request)
        {
            try
            {
                var logistics = new LogisticsUnit
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    Companies = request.Companies,
                    BrigadeId = request.BrigadeId,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    SupplyTrucks = request.SupplyTrucks,
                    FuelTrucks = request.FuelTrucks,
                    WaterTrucks = request.WaterTrucks,
                    AmmunitionTrucks = request.AmmunitionTrucks,
                    MaintenanceVehicles = request.MaintenanceVehicles,
                    RecoveryVehicles = request.RecoveryVehicles,
                    MobileWorkshops = request.MobileWorkshops,
                    FuelCapacity = request.FuelCapacity,
                    WaterCapacity = request.WaterCapacity,
                    HMG = request.HMG,
                    LMG = request.LMG,
                    SupplyState = request.SupplyState,
                    StrengthPercentage = request.StrengthPercentage,
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.LogisticsUnits.Add(logistics);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = logistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating logistics unit");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLogisticsUnit([FromBody] UpdateLogisticsUnitRequest request)
        {
            try
            {
                var existingUnit = await _context.LogisticsUnits
                    .FirstOrDefaultAsync(l => l.Id == request.Id && l.TeamId == user.TeamId);

                if (existingUnit == null) return NotFound();

                existingUnit.Name = request.Name;
                existingUnit.Description = request.Description;
                existingUnit.UnitCode = request.UnitCode;
                existingUnit.Strength = request.Strength;
                existingUnit.ForceType = request.ForceType;
                existingUnit.Companies = request.Companies;
                existingUnit.BrigadeId = request.BrigadeId;
                existingUnit.TokenId = request.TokenId;
                existingUnit.SupplyTrucks = request.SupplyTrucks;
                existingUnit.FuelTrucks = request.FuelTrucks;
                existingUnit.WaterTrucks = request.WaterTrucks;
                existingUnit.AmmunitionTrucks = request.AmmunitionTrucks;
                existingUnit.MaintenanceVehicles = request.MaintenanceVehicles;
                existingUnit.RecoveryVehicles = request.RecoveryVehicles;
                existingUnit.MobileWorkshops = request.MobileWorkshops;
                existingUnit.FuelCapacity = request.FuelCapacity;
                existingUnit.WaterCapacity = request.WaterCapacity;
                existingUnit.HMG = request.HMG;
                existingUnit.LMG = request.LMG;
                existingUnit.UpdatedBy = user.FullName;

                _context.Update(existingUnit);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingUnit });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating logistics unit");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenLogisticsUnit(Guid id)
        {
            try
            {
                var logistics = await _context.LogisticsUnits
                    .FirstOrDefaultAsync(l => l.Id == id && l.TeamId == user.TeamId);

                if (logistics == null) return NotFound();

                logistics.IsActive = false;
                logistics.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logistics unit");
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Combat Engineering Company Management (Token-Based)

        [HttpPost]
        public async Task<IActionResult> CreateCombatEngineeringCompany([FromBody] CreateEngineeringCompanyRequest request)
        {
            try
            {
                var engineering = new CombatEngineeringCompany
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    Platoons = request.Platoons,
                    BrigadeId = request.BrigadeId,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    EngineerVehicles = request.EngineerVehicles,
                    BridgeLayingVehicles = request.BridgeLayingVehicles,
                    MineClearingVehicles = request.MineClearingVehicles,
                    Bulldozers = request.Bulldozers,
                    Excavators = request.Excavators,
                    Cranes = request.Cranes,
                    ATGMS = request.ATGMS,
                    HMG = request.HMG,
                    LMG = request.LMG,
                    SupplyState = request.SupplyState,
                    StrengthPercentage = request.StrengthPercentage,
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.CombatEngineeringCompanies.Add(engineering);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = engineering });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating engineering company");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCombatEngineeringCompany([FromBody] UpdateEngineeringCompanyRequest request)
        {
            try
            {
                var existingUnit = await _context.CombatEngineeringCompanies
                    .FirstOrDefaultAsync(e => e.Id == request.Id && e.TeamId == user.TeamId);

                if (existingUnit == null) return NotFound();

                existingUnit.Name = request.Name;
                existingUnit.Description = request.Description;
                existingUnit.UnitCode = request.UnitCode;
                existingUnit.Strength = request.Strength;
                existingUnit.ForceType = request.ForceType;
                existingUnit.Platoons = request.Platoons;
                existingUnit.BrigadeId = request.BrigadeId;
                existingUnit.TokenId = request.TokenId;
                existingUnit.EngineerVehicles = request.EngineerVehicles;
                existingUnit.BridgeLayingVehicles = request.BridgeLayingVehicles;
                existingUnit.MineClearingVehicles = request.MineClearingVehicles;
                existingUnit.Bulldozers = request.Bulldozers;
                existingUnit.Excavators = request.Excavators;
                existingUnit.Cranes = request.Cranes;
                existingUnit.ATGMS = request.ATGMS;
                existingUnit.HMG = request.HMG;
                existingUnit.LMG = request.LMG;
                existingUnit.UpdatedBy = user.FullName;

                _context.Update(existingUnit);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingUnit });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating engineering company");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenCombatEngineeringCompany(Guid id)
        {
            try
            {
                var engineering = await _context.CombatEngineeringCompanies
                    .FirstOrDefaultAsync(e => e.Id == id && e.TeamId == user.TeamId);

                if (engineering == null) return NotFound();

                engineering.IsActive = false;
                engineering.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting engineering company");
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region ViewModels and Request Classes

        public class TokenDataEntryViewModel
        {
            public Token Token { get; set; }
            public Brigade ExistingBrigade { get; set; }
            public List<Brigade> AvailableBrigades { get; set; }
            public List<InfantryBattalion> ExistingInfantry { get; set; } = new List<InfantryBattalion>();
            public List<ArmouredRegiment> ExistingArmoured { get; set; } = new List<ArmouredRegiment>();
            public List<ArtilleryRegiment> ExistingArtillery { get; set; } = new List<ArtilleryRegiment>();
            public LogisticsUnit ExistingLogistics { get; set; }
            public CombatEngineeringCompany ExistingEngineering { get; set; }
            public List<Recon> ExistingRecon { get; set; } = new List<Recon>();
        }

        public class NewBrigadeDataEntryViewModel
        {
            public Token Token { get; set; }
            public Brigade Brigade { get; set; }
            public InfantryBattalion InfantryBattalion { get; set; }
            public ArmouredRegiment ArmouredRegiment { get; set; }
            public ArtilleryRegiment ArtilleryRegiment { get; set; }
        }

        public class UnitsDataEntryViewModel
        {
            public Token Token { get; set; }
            public Brigade Brigade { get; set; }
            public List<InfantryBattalion> ExistingInfantryList { get; set; } = new List<InfantryBattalion>();
            public List<ArmouredRegiment> ExistingArmouredList { get; set; } = new List<ArmouredRegiment>();
            public List<ArtilleryRegiment> ExistingArtilleryList { get; set; } = new List<ArtilleryRegiment>();
            public List<Intelligence> ExistingIntelligence { get; set; }
            public List<Recon> ExistingReconList { get; set; } = new List<Recon>();
            public List<LogisticsUnit> ExistingLogisticsList { get; set; } = new List<LogisticsUnit>();
            public List<CombatEngineeringCompany> ExistingEngineeringList { get; set; } = new List<CombatEngineeringCompany>();
        }

        public class TokenSummaryViewModel
        {
            public Token Token { get; set; }
            public Brigade Brigades { get; set; } = new Brigade();
            public List<InfantryBattalion> InfantryBattalions { get; set; } = new List<InfantryBattalion>();
            public List<ArmouredRegiment> ArmouredRegiments { get; set; } = new List<ArmouredRegiment>();
            public List<ArtilleryRegiment> ArtilleryRegiments { get; set; } = new List<ArtilleryRegiment>();
            public List<LogisticsUnit> LogisticsUnits { get; set; } = new List<LogisticsUnit>();
            public List<CombatEngineeringCompany> CombatEngineeringCompanies { get; set; } = new List<CombatEngineeringCompany>();
            public List<Recon> Recon { get; set; } = new List<Recon>();
        }

        public enum UnitType
        {
            Infantry,
            Armoured,
            Artillery,
            Recon,
            Logistics,
            Engineering
        }

        public class SingleUnitFormViewModel
        {
            public UnitType UnitType { get; set; }
            public Token Token { get; set; }
            public Brigade Brigade { get; set; }
            public Guid? TeamId { get; set; }
            public string ForceType { get; set; }
            public List<InfantryBattalion> ExistingInfantry { get; set; } = new List<InfantryBattalion>();
            public List<ArmouredRegiment> ExistingArmoured { get; set; } = new List<ArmouredRegiment>();
            public List<ArtilleryRegiment> ExistingArtillery { get; set; } = new List<ArtilleryRegiment>();
            public List<Recon> ExistingRecon { get; set; } = new List<Recon>();
        }

        public class DirectUnitFormViewModel
        {
            public UnitType UnitType { get; set; }
            public Token Token { get; set; }
            public Guid? TeamId { get; set; }
            public string ForceType { get; set; }
            public List<InfantryBattalion> ExistingInfantry { get; set; } = new List<InfantryBattalion>();
            public List<ArmouredRegiment> ExistingArmoured { get; set; } = new List<ArmouredRegiment>();
            public List<ArtilleryRegiment> ExistingArtillery { get; set; } = new List<ArtilleryRegiment>();
            public List<Recon> ExistingRecon { get; set; } = new List<Recon>();
            public LogisticsUnit ExistingLogistics { get; set; }
            public CombatEngineeringCompany ExistingEngineering { get; set; }
        }

        public class DirectUnitCreationViewModel
        {
            public UnitType UnitType { get; set; }
            public Token Token { get; set; }
            public Guid? TeamId { get; set; }
            public string ForceType { get; set; }
            public Guid? UnitId { get; set; }
            public string ExistingUnitJson { get; set; }
        }

        public class BrigadeUnitCreationViewModel
        {
            public UnitType UnitType { get; set; }
            public Token Token { get; set; }
            public Brigade Brigade { get; set; }
            public Guid? TeamId { get; set; }
            public string ForceType { get; set; }
            public Guid? UnitId { get; set; }
            public string ExistingUnitJson { get; set; }
        }

        public class CreateTokenBrigadeRequest
        {
            public string BrigadeCode { get; set; }
            public Guid TokenId { get; set; }
        }

        public class LinkBrigadeToTokenRequest
        {
            public Guid TokenId { get; set; }
            public Guid BrigadeId { get; set; }
        }

        public class CreateTokenInfantryBattalionRequest
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; } // Made optional for direct token units
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public int Companies { get; set; }
            public int ATGMS { get; set; }
            public int RocketLauncher { get; set; }
            public int Mortars81mm { get; set; }
            public int Mortars120mm { get; set; }
            public int GrenadeLaunchers { get; set; }
            public int HMG_AGL { get; set; }
            public int MG_LMG { get; set; }
            public int MANPADS { get; set; }
            public int Grenades { get; set; }
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
            public decimal MarchingSpeedTrucksRoads { get; set; }
            public decimal MarchingSpeedAPCs { get; set; }
            public decimal MarchingSpeedCrossCountry { get; set; }
            public decimal MarchingSpeedAPCsCrossCountry { get; set; }
            public decimal CombatAdvanceSpeed { get; set; }
        }

        public class CreateTokenArmouredRegimentRequest
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; } // Made optional for direct token units
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public int Squadrons { get; set; }
            public int Tanks { get; set; }
            public int ATGMS { get; set; }
            public int Mortars120mm { get; set; }
            public int HMG { get; set; }
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
            public decimal MarchingSpeedRoads { get; set; }
            public decimal MarchingSpeedCrossCountry { get; set; }
            public decimal CombatAdvanceSpeed { get; set; }
        }

        public class CreateTokenArtilleryRegimentRequest
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; } // Made optional for direct token units
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public int Batteries { get; set; }
            public int Guns { get; set; }
            public decimal GunRange { get; set; }
            public string GunCaliber { get; set; }
            public int HMG { get; set; }
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
        }

        public class CreateTokenReconRequest
        {
            public string ReconType { get; set; }
            public string Location { get; set; }
            public string Confidence { get; set; }
            public string Description { get; set; }
            public Guid TokenId { get; set; }
        }

        public class CreateLogisticsUnitRequest
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; }
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public int Companies { get; set; }
            public int SupplyTrucks { get; set; }
            public int FuelTrucks { get; set; }
            public int WaterTrucks { get; set; }
            public int AmmunitionTrucks { get; set; }
            public int MaintenanceVehicles { get; set; }
            public int RecoveryVehicles { get; set; }
            public int MobileWorkshops { get; set; }
            public int FuelCapacity { get; set; }
            public int WaterCapacity { get; set; }
            public int HMG { get; set; }
            public int LMG { get; set; }
            public int SupplyState { get; set; }
            public int StrengthPercentage { get; set; }
            public bool IsActive { get; set; }
        }

        public class UpdateLogisticsUnitRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; }
            public Guid TokenId { get; set; }
            public int Companies { get; set; }
            public int SupplyTrucks { get; set; }
            public int FuelTrucks { get; set; }
            public int WaterTrucks { get; set; }
            public int AmmunitionTrucks { get; set; }
            public int MaintenanceVehicles { get; set; }
            public int RecoveryVehicles { get; set; }
            public int MobileWorkshops { get; set; }
            public int FuelCapacity { get; set; }
            public int WaterCapacity { get; set; }
            public int HMG { get; set; }
            public int LMG { get; set; }
        }

        public class CreateEngineeringCompanyRequest
        {
            public Guid? Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; }
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public int Platoons { get; set; }
            public int EngineerVehicles { get; set; }
            public int BridgeLayingVehicles { get; set; }
            public int MineClearingVehicles { get; set; }
            public int Bulldozers { get; set; }
            public int Excavators { get; set; }
            public int Cranes { get; set; }
            public int ATGMS { get; set; }
            public int HMG { get; set; }
            public int LMG { get; set; }
            public int SupplyState { get; set; }
            public int StrengthPercentage { get; set; }
            public bool IsActive { get; set; }
        }

        public class UpdateEngineeringCompanyRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid? BrigadeId { get; set; }
            public Guid TokenId { get; set; }
            public int Platoons { get; set; }
            public int EngineerVehicles { get; set; }
            public int BridgeLayingVehicles { get; set; }
            public int MineClearingVehicles { get; set; }
            public int Bulldozers { get; set; }
            public int Excavators { get; set; }
            public int Cranes { get; set; }
            public int ATGMS { get; set; }
            public int HMG { get; set; }
            public int LMG { get; set; }
        }

        #endregion
    }
}
