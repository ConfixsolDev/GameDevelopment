using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class DataManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;

        public DataManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserSessionService userSessionService)
        {
            _context = context;
            _userManager = userManager;
            _userSessionService = userSessionService;
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
}
