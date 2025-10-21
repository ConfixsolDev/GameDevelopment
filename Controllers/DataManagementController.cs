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

                if (existingBrigade != null)
                {
                    // Load Infantry Battalions
                    viewModel.ExistingInfantry = await _context.InfantryBattalions
                        .Where(b => b.TokenId == tokenId && b.BrigadeId == existingBrigade.Id && b.TeamId == user.TeamId && b.IsActive)
                        .OrderByDescending(b => b.CreatedDate)
                        .ToListAsync();

                    // Load Armoured Regiments
                    viewModel.ExistingArmoured = await _context.ArmouredRegiments
                        .Where(r => r.TokenId == tokenId && r.BrigadeId == existingBrigade.Id && r.TeamId == user.TeamId && r.IsActive)
                        .OrderByDescending(r => r.CreatedDate)
                        .ToListAsync();

                    // Load Artillery Regiments
                    viewModel.ExistingArtillery = await _context.ArtilleryRegiments
                        .Where(r => r.TokenId == tokenId && r.BrigadeId == existingBrigade.Id && r.TeamId == user.TeamId && r.IsActive)
                        .OrderByDescending(r => r.CreatedDate)
                        .ToListAsync();

                    // Load Logistics Units
                    viewModel.ExistingLogistics = await _context.LogisticsUnits
                        .FirstOrDefaultAsync(l => l.TokenId == tokenId && l.BrigadeId == existingBrigade.Id && l.TeamId == user.TeamId && l.IsActive);

                    // Load Combat Engineering Companies
                    viewModel.ExistingEngineering = await _context.CombatEngineeringCompanies
                        .FirstOrDefaultAsync(c => c.TokenId == tokenId && c.BrigadeId == existingBrigade.Id && c.TeamId == user.TeamId && c.IsActive);
                }

                //// Load Reconnaissance (not tied to brigade)
                //viewModel.ExistingRecon = await _context.Recon
                //    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                //    .OrderByDescending(r => r.CreatedDate)
                //    .ToListAsync();

                return PartialView("Partials/_TokenDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading data entry form" });
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

                // Get existing units for this brigade
                var existingInfantry = await _context.InfantryBattalions
                    .FirstOrDefaultAsync(i => i.BrigadeId == brigadeId && i.IsActive);

                var existingArmoured = await _context.ArmouredRegiments
                    .FirstOrDefaultAsync(a => a.BrigadeId == brigadeId && a.IsActive);

                var existingArtillery = await _context.ArtilleryRegiments
                    .FirstOrDefaultAsync(ar => ar.BrigadeId == brigadeId && ar.IsActive);

                var existingRecon = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                var existingLogistics = await _context.LogisticsUnits
                    .Where(l => l.BrigadeId == brigade.Id && l.TeamId == user.TeamId && l.IsActive)
                    .FirstOrDefaultAsync();

                var existingEngineering = await _context.CombatEngineeringCompanies
                    .Where(e => e.BrigadeId == brigade.Id && e.TeamId == user.TeamId && e.IsActive)
                    .FirstOrDefaultAsync();

                var viewModel = new UnitsDataEntryViewModel
                {
                    Token = token,
                    Brigade = brigade,
                    ExistingInfantry = existingInfantry,
                    ExistingArmoured = existingArmoured,
                    ExistingArtillery = existingArtillery,
                    ExistingRecon = existingRecon,
                    ExistingLogistics = existingLogistics,
                    ExistingEngineering = existingEngineering
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

                //Brigades.Recon = _context.Recon
                //    .AsNoTracking()
                //    .Where(r => r.TokenId == tokenId  && r.IsActive)
                //    .OrderByDescending(r => r.CreatedDate)
                //    .ToList();

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
                    Recon = Brigades.Recon ?? new List<Recon>()
                };

                return PartialView("Partials/_TokenSummaryModal", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", ViewBag.ErrorMessage);
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

                if (brigade == null) 
                {
                    return Json(new { success = false, message = "Brigade not found or not accessible." });
                }

                brigade.TokenId = request.TokenId;
                brigade.UpdatedBy = user.FullName;
                
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
                existingRegiment.Guns = regiment.Guns;
                existingRegiment.GunRange = regiment.GunRange;
                existingRegiment.GunCaliber = regiment.GunCaliber;
                existingRegiment.HMG = regiment.HMG;
                existingRegiment.Drones = regiment.Drones;
                existingRegiment.DroneTypes = regiment.DroneTypes;
                existingRegiment.UpdatedBy = user.FullName;

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
            public InfantryBattalion ExistingInfantry { get; set; }
            public ArmouredRegiment ExistingArmoured { get; set; }
            public ArtilleryRegiment ExistingArtillery { get; set; }
            public List<Intelligence> ExistingIntelligence { get; set; }
            public List<Recon> ExistingRecon { get; set; }
            public LogisticsUnit ExistingLogistics { get; set; }
            public CombatEngineeringCompany ExistingEngineering { get; set; }
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
            Recon
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
            public Guid BrigadeId { get; set; }
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
            public Guid BrigadeId { get; set; }
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
            public Guid BrigadeId { get; set; }
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

        #endregion
    }
}
