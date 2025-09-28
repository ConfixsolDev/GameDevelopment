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
    [AuthorizeDynamic]
    public class DataManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationUserVM user;

        public DataManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserSessionService userSessionService)
        {
            _context = context;
            _userManager = userManager;
            user = userSessionService.GetCurrentUser();
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Data Management";
            ViewData["Subtitle"] = "Military Unit Data Entry System";
            return View();
        }

        #region Brigade Management

        [HttpGet]
        public async Task<IActionResult> GetBrigades()
        {
            var teamId = user?.TeamId;

            var brigades = await _context.Brigades
                .Where(b => b.TeamId == teamId && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Json(brigades);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrigade([FromBody] Brigade brigade)
        {
            try
            {
                brigade.Id = Guid.NewGuid();
                brigade.CreatedBy = user.FullName;
                brigade.TeamId = user.TeamId;
                brigade.IsActive = true;

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
        public async Task<IActionResult> UpdateBrigade([FromBody] Brigade brigade)
        {
            try
            {
                var existingBrigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == brigade.Id && b.TeamId == user.TeamId);

                if (existingBrigade == null) return NotFound();

                existingBrigade.Name = brigade.Name;
                existingBrigade.Description = brigade.Description;
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
        public async Task<IActionResult> DeleteBrigade(Guid id)
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

        #endregion

        #region Infantry Battalion Management

        [HttpGet]
        public async Task<IActionResult> GetInfantryBattalions()
        {
            var teamId = user?.TeamId;

            var battalions = await _context.InfantryBattalions
                .Where(b => b.TeamId == teamId && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Json(battalions);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInfantryBattalion([FromBody] InfantryBattalion battalion)
        {
            try
            {
                if (user == null) return Unauthorized();

                battalion.Id = Guid.NewGuid();
                battalion.CreatedBy = user.FullName;
                battalion.TeamId = user.TeamId;
                battalion.IsActive = true;

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
        public async Task<IActionResult> UpdateInfantryBattalion([FromBody] InfantryBattalion battalion)
        {
            try
            {
                if (user == null) return Unauthorized();

                var existingBattalion = await _context.InfantryBattalions
                    .FirstOrDefaultAsync(b => b.Id == battalion.Id && b.TeamId == user.TeamId);

                if (existingBattalion == null) return NotFound();

                // Update all properties
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
        public async Task<IActionResult> DeleteInfantryBattalion(Guid id)
        {
            try
            {
                if (user == null) return Unauthorized();

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

        #region Armoured Regiment Management

        [HttpGet]
        public async Task<IActionResult> GetArmouredRegiments()
        {
            var teamId = user?.TeamId;

            var regiments = await _context.ArmouredRegiments
                .Where(r => r.TeamId == teamId && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Json(regiments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArmouredRegiment([FromBody] ArmouredRegiment regiment)
        {
            try
            {
                if (user == null) return Unauthorized();

                regiment.Id = Guid.NewGuid();
                regiment.TeamId = user.TeamId;
                regiment.IsActive = true;

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
        public async Task<IActionResult> UpdateArmouredRegiment([FromBody] ArmouredRegiment regiment)
        {
            try
            {
                if (user == null) return Unauthorized();

                var existingRegiment = await _context.ArmouredRegiments
                    .FirstOrDefaultAsync(r => r.Id == regiment.Id && r.TeamId == user.TeamId);

                if (existingRegiment == null) return NotFound();

                // Update all properties
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
        public async Task<IActionResult> DeleteArmouredRegiment(Guid id)
        {
            try
            {
                if (user == null) return Unauthorized();

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

        #region Artillery Regiment Management

        [HttpGet]
        public async Task<IActionResult> GetArtilleryRegiments()
        {
            var teamId = user?.TeamId;

            var regiments = await _context.ArtilleryRegiments
                .Where(r => r.TeamId == teamId && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Json(regiments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateArtilleryRegiment([FromBody] ArtilleryRegiment regiment)
        {
            try
            {
                if (user == null) return Unauthorized();

                regiment.Id = Guid.NewGuid();
                regiment.CreatedBy = user.FullName;
                regiment.TeamId = user.TeamId;
                regiment.IsActive = true;

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
        public async Task<IActionResult> UpdateArtilleryRegiment([FromBody] ArtilleryRegiment regiment)
        {
            try
            {
                if (user == null) return Unauthorized();

                var existingRegiment = await _context.ArtilleryRegiments
                    .FirstOrDefaultAsync(r => r.Id == regiment.Id && r.TeamId == user.TeamId);

                if (existingRegiment == null) return NotFound();

                // Update all properties
                existingRegiment.Name = regiment.Name;
                existingRegiment.Description = regiment.Description;
                existingRegiment.UnitCode = regiment.UnitCode;
                existingRegiment.Strength = regiment.Strength;
                existingRegiment.ForceType = regiment.ForceType;
                existingRegiment.Batteries = regiment.Batteries;
                existingRegiment.Guns = regiment.Guns;
                existingRegiment.GunRange = regiment.GunRange;
                existingRegiment.HMG = regiment.HMG;
                existingRegiment.GunCaliber = regiment.GunCaliber;
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
        public async Task<IActionResult> DeleteArtilleryRegiment(Guid id)
        {
            try
            {
                if (user == null) return Unauthorized();

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

        #region Terrain Mobility Factor Management

        [HttpGet]
        public async Task<IActionResult> GetTerrainMobilityFactors()
        {
            var teamId = user?.TeamId;

            var factors = await _context.TerrainMobilityFactors
                .Where(t => t.TeamId == teamId && t.IsActive)
                .OrderBy(t => t.TerrainType)
                .ToListAsync();

            return Json(factors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTerrainMobilityFactor([FromBody] TerrainMobilityFactor factor)
        {
            try
            {
                if (user == null) return Unauthorized();

                factor.Id = Guid.NewGuid();
                factor.CreatedBy = user.FullName;
                factor.TeamId = user.TeamId;
                factor.IsActive = true;

                _context.TerrainMobilityFactors.Add(factor);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = factor });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Force Protection Management

        [HttpGet]
        public async Task<IActionResult> GetForceProtections()
        {
            var teamId = user?.TeamId;

            var protections = await _context.ForceProtections
                .Where(f => f.TeamId == teamId && f.IsActive)
                .OrderBy(f => f.ForceType)
                .ThenBy(f => f.ProtectionType)
                .ToListAsync();

            return Json(protections);
        }

        [HttpPost]
        public async Task<IActionResult> CreateForceProtection([FromBody] ForceProtection protection)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                protection.Id = Guid.NewGuid();
                protection.CreatedBy = user.FullName;
                protection.TeamId = user.TeamId;
                protection.IsActive = true;

                _context.ForceProtections.Add(protection);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = protection });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Token-Brigade Integration

        [HttpGet]
        public async Task<IActionResult> GetBrigadeByToken(Guid tokenId)
        {
            try
            {
                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive);

                if (brigade == null)
                {
                    return Json(new { success = false, message = "No brigade data found for this token" });
                }

                return Json(new { success = true, data = brigade });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrigadeForToken([FromBody] CreateBrigadeForTokenRequest request)
        {
            try
            {
                var brigade = new Brigade
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    BrigadeCode = request.BrigadeCode,
                    ForceType = request.ForceType,
                    TokenId = request.TokenId,
                    CreatedBy = user.FullName,
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

        #endregion

        #region Data Export/Import

        [HttpGet]
        public async Task<IActionResult> ExportTeamData()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var teamId = user.TeamId;
                var teamData = new
                {
                    Brigades = await _context.Brigades.Where(b => b.TeamId == teamId && b.IsActive).ToListAsync(),
                    InfantryBattalions = await _context.InfantryBattalions.Where(b => b.TeamId == teamId && b.IsActive).ToListAsync(),
                    ArmouredRegiments = await _context.ArmouredRegiments.Where(r => r.TeamId == teamId && r.IsActive).ToListAsync(),
                    ArtilleryRegiments = await _context.ArtilleryRegiments.Where(r => r.TeamId == teamId && r.IsActive).ToListAsync(),
                    TerrainMobilityFactors = await _context.TerrainMobilityFactors.Where(t => t.TeamId == teamId && t.IsActive).ToListAsync(),
                    ForceProtections = await _context.ForceProtections.Where(f => f.TeamId == teamId && f.IsActive).ToListAsync()
                };

                return Json(new { success = true, data = teamData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Token-Specific Data Management

        [HttpGet]
        public async Task<IActionResult> GetTokenSummary(Guid tokenId)
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

                // Get all military data for this token
                var brigades = await _context.Brigades
                    .Where(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive)
                    .OrderByDescending(b => b.CreatedDate)
                    .ToListAsync();

                var infantry = await _context.InfantryBattalions
                    .Where(i => i.TokenId == tokenId && i.TeamId == user.TeamId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var armoured = await _context.ArmouredRegiments
                    .Where(a => a.TokenId == tokenId && a.TeamId == user.TeamId && a.IsActive)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var artillery = await _context.ArtilleryRegiments
                    .Where(a => a.TokenId == tokenId && a.TeamId == user.TeamId && a.IsActive)
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                var intelligence = await _context.Intelligence
                    .Where(i => i.TokenId == tokenId && i.TeamId == user.TeamId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var recon = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                // Filter units by the brigades belonging to this token
                var brigadeIds = brigades.Select(b => b.Id).ToHashSet();
                var infantryForToken = infantry.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();
                var armouredForToken = armoured.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();
                var artilleryForToken = artillery.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();

                // Create ViewModel
                var viewModel = new TokenSummaryViewModel
                {
                    Token = token,
                    Brigades = brigades,
                    InfantryBattalions = infantryForToken,
                    ArmouredRegiments = armouredForToken,
                    ArtilleryRegiments = artilleryForToken,
                    Intelligence = intelligence,
                    Recon = recon
                };

                return PartialView("Partials/_TokenSummaryModal", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading token summary" });
            }
        }
       

        [HttpGet]
        public async Task<IActionResult> GetTokenBrigades(Guid tokenId)
        {
            try
            {
                var brigades = await _context.Brigades
                    .Where(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive)
                    .OrderBy(b => b.Name)
                    .ToListAsync();

                return Json(new { success = true, data = brigades });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTokenBrigade([FromBody] CreateTokenBrigadeRequest request)
        {
            try
            {
                var brigade = new Brigade
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    BrigadeCode = request.BrigadeCode,
                    ForceType = request.ForceType,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    CreatedBy = user.FullName,
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

                existingBrigade.Name = brigade.Name;
                existingBrigade.Description = brigade.Description;
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

        [HttpGet]
        public async Task<IActionResult> GetTokenInfantryBattalions(Guid tokenId, Guid? brigadeId = null)
        {
            try
            {
                var query = _context.InfantryBattalions
                    .Where(b => b.TeamId == user.TeamId && b.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(b => b.BrigadeId == brigadeId.Value);
                }

                var battalions = await query
                    .OrderBy(b => b.Name)
                    .ToListAsync();

                return Json(new { success = true, data = battalions });
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

        [HttpGet]
        public async Task<IActionResult> GetTokenArmouredRegiments(Guid tokenId, Guid? brigadeId = null)
        {
            try
            {
                var query = _context.ArmouredRegiments
                    .Where(r => r.TeamId == user.TeamId && r.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(r => r.BrigadeId == brigadeId.Value);
                }

                var regiments = await query
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return Json(new { success = true, data = regiments });
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

        [HttpGet]
        public async Task<IActionResult> GetTokenArtilleryRegiments(Guid tokenId, Guid? brigadeId = null)
        {
            try
            {
                var query = _context.ArtilleryRegiments
                    .Where(r => r.TeamId == user.TeamId && r.IsActive);

                if (brigadeId.HasValue)
                {
                    query = query.Where(r => r.BrigadeId == brigadeId.Value);
                }

                var regiments = await query
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return Json(new { success = true, data = regiments });
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

        [HttpGet]
        public async Task<IActionResult> GetTokenIntelligence(Guid tokenId)
        {
            try
            {
                var intelligence = await _context.Intelligence
                    .Where(i => i.TokenId == tokenId && i.TeamId == user.TeamId && i.IsActive)
                    .OrderByDescending(i => i.Timestamp)
                    .ToListAsync();

                return Json(new { success = true, data = intelligence });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTokenIntelligence(Guid id)
        {
            try
            {
                
                var intelligence = await _context.Intelligence
                    .FirstOrDefaultAsync(i => i.Id == id && i.TeamId == user.TeamId);

                if (intelligence == null) return NotFound();

                intelligence.IsActive = false;
                intelligence.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenRecon(Guid tokenId)
        {
            try
            {
               
                var recon = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.Timestamp)
                    .ToListAsync();

                return Json(new { success = true, data = recon });
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

        [HttpPost]
        public async Task<IActionResult> CreateTokenInfantryBattalion([FromBody] CreateTokenInfantryBattalionRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
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

        [HttpPost]
        public async Task<IActionResult> CreateTokenArmouredRegiment([FromBody] CreateTokenArmouredRegimentRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
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

        [HttpPost]
        public async Task<IActionResult> CreateTokenArtilleryRegiment([FromBody] CreateTokenArtilleryRegimentRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
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

        [HttpPost]
        public async Task<IActionResult> CreateTokenIntelligence([FromBody] CreateTokenIntelligenceRequest request)
        {
            try
            {
                
                var intelligence = new Intelligence
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    Source = request.Source,
                    Priority = request.Priority,
                    TokenId = request.TokenId,
                    TeamId = user.TeamId,
                    CreatedBy = user.FullName,
                    IsActive = true
                };

                _context.Intelligence.Add(intelligence);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = intelligence });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenIntelligence([FromBody] Intelligence intelligence)
        {
            try
            {
                var existingIntelligence = await _context.Intelligence
                    .FirstOrDefaultAsync(i => i.Id == intelligence.Id && i.TeamId == user.TeamId);

                if (existingIntelligence == null) return NotFound();

                existingIntelligence.Title = intelligence.Title;
                existingIntelligence.Description = intelligence.Description;
                existingIntelligence.Source = intelligence.Source;
                existingIntelligence.Priority = intelligence.Priority;
                existingIntelligence.UpdatedBy = user.FullName;

                await _context.SaveChangesAsync();

                return Json(new { success = true, data = existingIntelligence });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

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

        [HttpPost]
        public async Task<IActionResult> LinkBrigadeToToken([FromBody] LinkBrigadeToTokenRequest request)
        {
            try
            {
                // Find the brigade
                var brigade = await _context.Brigades
                    .FirstOrDefaultAsync(b => b.Id == request.BrigadeId && b.TeamId == user.TeamId && b.IsActive);

                if (brigade == null) 
                {
                    return Json(new { success = false, message = "Brigade not found or not accessible." });
                }

                // Link the brigade to the token
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
                    .Include(b => b.InfantryBattalions)
                    .Include(b => b.ArmouredRegiments)
                    .Include(b => b.ArtilleryRegiments)
                    .FirstOrDefaultAsync(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive);

                var viewModel = new TokenDataEntryViewModel
                {
                    Token = token,
                    ExistingBrigade = existingBrigade,
                    AvailableBrigades = await _context.Brigades
                        .Where(b => b.TeamId == user.TeamId && b.IsActive && b.TokenId == null)
                        .OrderBy(b => b.Name)
                        .ToListAsync()
                };

                return PartialView("Partials/_TokenDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading data entry form" });
            }
        }

        /// <summary>
        /// Returns the units data entry form for a specific brigade
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UnitsDataEntryForm(Guid tokenId, Guid brigadeId)
        {
            try
            {

                // Get the token details
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == user.TeamId && t.IsActive);

                if (token == null)
                {
                    return PartialView("Partials/_ErrorPartial", new { Message = "Token not found or not accessible" });
                }

                // Get the brigade details
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

                // Get existing Intelligence and Recon data for this token
                var existingIntelligence = await _context.Intelligence
                    .Where(i => i.TokenId == tokenId && i.TeamId == user.TeamId && i.IsActive)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var existingRecon = await _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                var viewModel = new UnitsDataEntryViewModel
                {
                    Token = token,
                    Brigade = brigade,
                    ExistingInfantry = existingInfantry,
                    ExistingArmoured = existingArmoured,
                    ExistingArtillery = existingArtillery,
                    ExistingIntelligence = existingIntelligence,
                    ExistingRecon = existingRecon
                };

                return PartialView("Partials/_UnitsDataEntryForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("Partials/_ErrorPartial", new { Message = "Error loading units form" });
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
                // Get the token details
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
                        ForceType = "Blue" // Default, can be changed
                    },
                    InfantryBattalion = new InfantryBattalion 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = "Blue"
                    },
                    ArmouredRegiment = new ArmouredRegiment 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = "Blue"
                    },
                    ArtilleryRegiment = new ArtilleryRegiment 
                    { 
                        TokenId = tokenId,
                        TeamId = user.TeamId,
                        ForceType = "Blue"
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
        /// Create a new brigade only (without units) for a token
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTokenBrigade([FromBody] CreateBrigadeOnlyRequest request)
        {
            try
            {
                // Create the brigade
                var brigade = new Brigade
                {
                    Id = Guid.NewGuid(),
                    Name = request.BrigadeName,
                    BrigadeCode = request.BrigadeCode,
                    Description = request.BrigadeDescription,
                    ForceType = request.ForceType,
                    TokenId = request.TokenId,
                    TeamId = request.TeamId,
                    CreatedBy = user.FullName,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Brigades.Add(brigade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Brigade created successfully", brigadeId = brigade.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating brigade: " + ex.Message });
            }
        }

        /// <summary>
        /// Create a new brigade with all units for a token (legacy - replaced by separate approach)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTokenBrigadeWithUnits([FromBody] CreateTokenBrigadeRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Create the brigade
                var brigade = new Brigade
                {
                    Id = Guid.NewGuid(),
                    Name = request.BrigadeName,
                    BrigadeCode = request.BrigadeCode,
                    Description = request.BrigadeDescription,
                    ForceType = request.ForceType,
                    TokenId = request.TokenId,
                    TeamId = request.TeamId,
                    CreatedBy = user.FullName,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Brigades.Add(brigade);

                // Create Infantry Battalion if data provided
                if (!string.IsNullOrEmpty(request.InfantryName))
                {
                    var infantry = new InfantryBattalion
                    {
                        Id = Guid.NewGuid(),
                        Name = request.InfantryName,
                        UnitCode = request.InfantryCode,
                        Description = request.InfantryDescription,
                        Strength = request.InfantryStrength,
                        ForceType = request.ForceType,
                        BrigadeId = brigade.Id,
                        TokenId = request.TokenId,
                        TeamId = request.TeamId,
                        Companies = request.InfantryCompanies,
                        ATGMS = request.InfantryATGMS,
                        RocketLauncher = request.InfantryRocketLauncher,
                        Mortars81mm = request.InfantryMortars81mm,
                        Mortars120mm = request.InfantryMortars120mm,
                        GrenadeLaunchers = request.InfantryGrenadeLaunchers,
                        HMG_AGL = request.InfantryHMG_AGL,
                        MG_LMG = request.InfantryMG_LMG,
                        MANPADS = request.InfantryMANPADS,
                        Grenades = request.InfantryGrenades,
                        Drones = request.InfantryDrones,
                        DroneTypes = request.InfantryDroneTypes,
                        CreatedBy = user.FullName,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.InfantryBattalions.Add(infantry);
                }

                // Create Armoured Regiment if data provided
                if (!string.IsNullOrEmpty(request.ArmouredName))
                {
                    var armoured = new ArmouredRegiment
                    {
                        Id = Guid.NewGuid(),
                        Name = request.ArmouredName,
                        UnitCode = request.ArmouredCode,
                        Description = request.ArmouredDescription,
                        Strength = request.ArmouredStrength,
                        ForceType = request.ForceType,
                        BrigadeId = brigade.Id,
                        TokenId = request.TokenId,
                        TeamId = request.TeamId,
                        Squadrons = request.ArmouredSquadrons,
                        Tanks = request.ArmouredTanks,
                        ATGMS = request.ArmouredATGMS,
                        Mortars120mm = request.ArmouredMortars120mm,
                        HMG = request.ArmouredHMG,
                        Drones = request.ArmouredDrones,
                        DroneTypes = request.ArmouredDroneTypes,
                        CreatedBy = user.FullName,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.ArmouredRegiments.Add(armoured);
                }

                // Create Artillery Regiment if data provided
                if (!string.IsNullOrEmpty(request.ArtilleryName))
                {
                    var artillery = new ArtilleryRegiment
                    {
                        Id = Guid.NewGuid(),
                        Name = request.ArtilleryName,
                        UnitCode = request.ArtilleryCode,
                        Description = request.ArtilleryDescription,
                        Strength = request.ArtilleryStrength,
                        ForceType = request.ForceType,
                        BrigadeId = brigade.Id,
                        TokenId = request.TokenId,
                        TeamId = request.TeamId,
                        Batteries = request.ArtilleryBatteries,
                        Guns = request.ArtilleryGuns,
                        GunRange = request.ArtilleryGunRange,
                        GunCaliber = request.ArtilleryGunCaliber,
                        HMG = request.ArtilleryHMG,
                        Drones = request.ArtilleryDrones,
                        DroneTypes = request.ArtilleryDroneTypes,
                        CreatedBy = user.FullName,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.ArtilleryRegiments.Add(artillery);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Brigade and units created successfully", brigadeId = brigade.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating brigade: " + ex.Message });
            }
        }

        public class CreateBrigadeOnlyRequest
        {
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            public string BrigadeName { get; set; }
            public string BrigadeCode { get; set; }
            public string BrigadeDescription { get; set; }
            public string ForceType { get; set; }
        }

        public class CreateTokenBrigadeRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Guid TokenId { get; set; }
            public Guid TeamId { get; set; }
            
            // Brigade data
            public string BrigadeName { get; set; }
            public string BrigadeCode { get; set; }
            public string BrigadeDescription { get; set; }
            public string ForceType { get; set; }
            
            // Infantry data
            public string InfantryName { get; set; }
            public string InfantryCode { get; set; }
            public string InfantryDescription { get; set; }
            public int InfantryStrength { get; set; }
            public int InfantryCompanies { get; set; }
            public int InfantryATGMS { get; set; }
            public int InfantryRocketLauncher { get; set; }
            public int InfantryMortars81mm { get; set; }
            public int InfantryMortars120mm { get; set; }
            public int InfantryGrenadeLaunchers { get; set; }
            public int InfantryHMG_AGL { get; set; }
            public int InfantryMG_LMG { get; set; }
            public int InfantryMANPADS { get; set; }
            public int InfantryGrenades { get; set; }
            public int InfantryDrones { get; set; }
            public string InfantryDroneTypes { get; set; }
            
            // Armoured data
            public string ArmouredName { get; set; }
            public string ArmouredCode { get; set; }
            public string ArmouredDescription { get; set; }
            public int ArmouredStrength { get; set; }
            public int ArmouredSquadrons { get; set; }
            public int ArmouredTanks { get; set; }
            public int ArmouredATGMS { get; set; }
            public int ArmouredMortars120mm { get; set; }
            public int ArmouredHMG { get; set; }
            public int ArmouredDrones { get; set; }
            public string ArmouredDroneTypes { get; set; }
            
            // Artillery data
            public string ArtilleryName { get; set; }
            public string ArtilleryCode { get; set; }
            public string ArtilleryDescription { get; set; }
            public int ArtilleryStrength { get; set; }
            public int ArtilleryBatteries { get; set; }
            public int ArtilleryGuns { get; set; }
            public decimal ArtilleryGunRange { get; set; }
            public string ArtilleryGunCaliber { get; set; }
            public int ArtilleryHMG { get; set; }
            public int ArtilleryDrones { get; set; }
            public string ArtilleryDroneTypes { get; set; }
        }

        public class LinkBrigadeToTokenRequest
        {
            public Guid TokenId { get; set; }
            public Guid BrigadeId { get; set; }
        }

        public class TokenDataEntryViewModel
        {
            public Token Token { get; set; }
            public Brigade ExistingBrigade { get; set; }
            public List<Brigade> AvailableBrigades { get; set; }
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
        }

        public class TokenSummaryViewModel
        {
            public Token Token { get; set; }
            public List<Brigade> Brigades { get; set; }
            public List<InfantryBattalion> InfantryBattalions { get; set; }
            public List<ArmouredRegiment> ArmouredRegiments { get; set; }
            public List<ArtilleryRegiment> ArtilleryRegiments { get; set; }
            public List<Intelligence> Intelligence { get; set; }
            public List<Recon> Recon { get; set; }
        }
        // Request DTOs
        public class CreateBrigadeForTokenRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string BrigadeCode { get; set; }
            public string ForceType { get; set; }
            public Guid TokenId { get; set; }
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
            // Equipment & Weapons
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
            // Drone Data
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
            // Mobility Data
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
            // Equipment & Weapons
            public int Squadrons { get; set; }
            public int Tanks { get; set; }
            public int ATGMS { get; set; }
            public int Mortars120mm { get; set; }
            public int HMG { get; set; }
            // Drone Data
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
            // Mobility Data
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
            // Artillery Equipment
            public int Batteries { get; set; }
            public int Guns { get; set; }
            public decimal GunRange { get; set; }
            public string GunCaliber { get; set; }
            public int HMG { get; set; }
            // Drone Data
            public int Drones { get; set; }
            public string DroneTypes { get; set; }
        }

        public class CreateTokenIntelligenceRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Source { get; set; }
            public string Priority { get; set; }
            public Guid TokenId { get; set; }
        }

        public class CreateTokenReconRequest
        {
            public string ReconType { get; set; }
            public string Location { get; set; }
            public string Confidence { get; set; }
            public string Description { get; set; }
            public Guid TokenId { get; set; }
        }

    }
}
