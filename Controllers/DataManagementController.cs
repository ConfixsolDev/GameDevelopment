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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
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
            var user = await _userManager.GetUserAsync(User);
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
        public  IActionResult GetTokenSummary(Guid tokenId)
        {
            try
            {
                var brigadesTask = _context.Brigades
                    .Where(b => b.TokenId == tokenId && b.TeamId == user.TeamId && b.IsActive)
                    .OrderBy(b => b.Name)
                    .ToList();

                var infantryTask = _context.InfantryBattalions
                    .Where(b => b.TeamId == user.TeamId && b.IsActive)
                    .ToList();

                var armouredTask = _context.ArmouredRegiments
                    .Where(r => r.TeamId == user.TeamId && r.IsActive)
                    .ToList();

                var artilleryTask = _context.ArtilleryRegiments
                    .Where(r => r.TeamId == user.TeamId && r.IsActive)
                    .ToList();

                var intelTask = _context.Intelligence
                    .Where(i => i.TokenId == tokenId && i.TeamId == user.TeamId && i.IsActive)
                    .OrderByDescending(i => i.Timestamp)
                    .ToList();

                var reconTask = _context.Recon
                    .Where(r => r.TokenId == tokenId && r.TeamId == user.TeamId && r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToList();

                var brigades = brigadesTask;
                var infantry = infantryTask;
                var armoured = armouredTask;
                var artillery = artilleryTask;

                // Filter units by the brigades belonging to this token
                var brigadeIds = brigades.Select(b => b.Id).ToHashSet();
                var infantryForToken = infantry.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();
                var armouredForToken = armoured.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();
                var artilleryForToken = artillery.Where(u => u.BrigadeId.HasValue && brigadeIds.Contains(u.BrigadeId.Value)).ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        brigades = brigades,
                        infantryBattalions = infantryForToken,
                        armouredRegiments = armouredForToken,
                        artilleryRegiments = artilleryForToken,
                        intelligence = intelTask,
                        recon = reconTask
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenBrigades(Guid tokenId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
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

        [HttpGet]
        public async Task<IActionResult> GetTokenArmouredRegiments(Guid tokenId, Guid? brigadeId = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
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

        [HttpGet]
        public async Task<IActionResult> GetTokenArtilleryRegiments(Guid tokenId, Guid? brigadeId = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
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

        [HttpGet]
        public async Task<IActionResult> GetTokenIntelligence(Guid tokenId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TeamId = user.TeamId,
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TeamId = user.TeamId,
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    UnitCode = request.UnitCode,
                    Strength = request.Strength,
                    ForceType = request.ForceType,
                    BrigadeId = request.BrigadeId,
                    TeamId = user.TeamId,
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

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

        // Request DTOs
        public class CreateBrigadeForTokenRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string BrigadeCode { get; set; }
            public string ForceType { get; set; }
            public Guid TokenId { get; set; }
        }

        public class CreateTokenBrigadeRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string BrigadeCode { get; set; }
            public string ForceType { get; set; }
            public Guid TokenId { get; set; }
        }

        public class CreateTokenInfantryBattalionRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid BrigadeId { get; set; }
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
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid BrigadeId { get; set; }
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
            public string Name { get; set; }
            public string Description { get; set; }
            public string UnitCode { get; set; }
            public int Strength { get; set; }
            public string ForceType { get; set; }
            public Guid BrigadeId { get; set; }
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

        public class LinkBrigadeToTokenRequest
        {
            public Guid TokenId { get; set; }
            public Guid BrigadeId { get; set; }
        }
    }
}
