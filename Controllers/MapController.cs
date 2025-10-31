using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace TechWebSol.Controllers
{
    [Authorize]

    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<MapController> _logger;
        private readonly IWebHostEnvironment _env;

        public MapController(
            ApplicationDbContext context,
            IUserSessionService _userSessionService,
            ILogger<MapController> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            this._userSessionService = _userSessionService;
            _logger = logger;
            _env = env;
        }

        #region Map Regions

        [HttpGet]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var regions = await _context.MapRegions
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Name)
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        geometry = r.Geometry,
                        properties = r.Properties,
                        areaM2 = r.AreaM2,
                        centerLat = r.CenterLat,
                        centerLng = r.CenterLng,
                        regionType = r.RegionType,
                        description = r.Description,
                        isLocked = r.IsLocked,
                        lastModified = r.LastModified
                    })
                    .ToListAsync();

                return Json(new { success = true, data = regions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regions");
                return Json(new { success = false, message = "Error retrieving regions" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRegion([FromBody] SaveRegionRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var region = new MapRegion
                {
                    Name = request.Name,
                    Geometry = request.Geometry,
                    Properties = request.Properties,
                    AreaM2 = request.AreaM2,
                    CenterLat = request.CenterLat,
                    CenterLng = request.CenterLng,
                    RegionType = request.RegionType ?? "main",
                    Description = request.Description,
                    IsLocked = request.IsLocked,
                    LastModified = DateTime.Now,
                    CreatedBy = currentUser.ApplicationUserId.ToString(),
                    TeamId = currentUser.TeamId
                };

                _context.MapRegions.Add(region);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = new { id = region.Id }, message = "Region saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving region");
                return Json(new { success = false, message = "Error saving region" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRegion([FromBody] UpdateRegionRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var region = await _context.MapRegions.FindAsync(request.Id);
                if (region == null)
                {
                    return Json(new { success = false, message = "Region not found" });
                }

                region.Name = request.Name;
                region.Geometry = request.Geometry;
                region.Properties = request.Properties;
                region.AreaM2 = request.AreaM2;
                region.CenterLat = request.CenterLat;
                region.CenterLng = request.CenterLng;
                region.RegionType = request.RegionType;
                region.Description = request.Description;
                region.IsLocked = request.IsLocked;
                region.LastModified = DateTime.Now;
                region.UpdatedBy = currentUser.ApplicationUserId.ToString();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Region updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating region");
                return Json(new { success = false, message = "Error updating region" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRegion(Guid id)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var region = await _context.MapRegions.FindAsync(id);
                if (region == null)
                {
                    return Json(new { success = false, message = "Region not found" });
                }

                region.IsActive = false;
                region.UpdatedBy = currentUser.ApplicationUserId.ToString();
                region.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Region deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region");
                return Json(new { success = false, message = "Error deleting region" });
            }
        }

        #endregion

        #region Map Sectors

        [HttpGet]
        public async Task<IActionResult> GetSectors()
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var sectors = await _context.MapSectors
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        geometry = s.Geometry,
                        landType = s.LandType,
                        properties = s.Properties,
                        areaM2 = s.AreaM2,
                        centerLat = s.CenterLat,
                        centerLng = s.CenterLng,
                        sectorType = s.SectorType,
                        description = s.Description,
                        lastModified = s.LastModified
                    })
                    .ToListAsync();

                return Json(new { success = true, data = sectors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sectors");
                return Json(new { success = false, message = "Error retrieving sectors" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSector([FromBody] SaveSectorRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var sector = new MapSector
                {
                    Name = request.Name,
                    Geometry = request.Geometry,
                    LandType = request.LandType,
                    Properties = request.Properties,
                    AreaM2 = request.AreaM2,
                    CenterLat = request.CenterLat,
                    CenterLng = request.CenterLng,
                    SectorType = request.SectorType,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    CreatedBy = currentUser.ApplicationUserId.ToString(),
                    TeamId = currentUser.TeamId
                };

                _context.MapSectors.Add(sector);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = new { id = sector.Id }, message = "Sector saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sector");
                return Json(new { success = false, message = "Error saving sector" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSector(Guid id)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var sector = await _context.MapSectors.FindAsync(id);
                if (sector == null)
                {
                    return Json(new { success = false, message = "Sector not found" });
                }

                sector.IsActive = false;
                sector.UpdatedBy = currentUser.ApplicationUserId.ToString();
                sector.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Sector deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sector");
                return Json(new { success = false, message = "Error deleting sector" });
            }
        }

        #endregion

        #region Map Labels

        [HttpGet]
        public async Task<IActionResult> GetLabels()
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var labels = await _context.MapLabels
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.Text)
                    .Select(l => new
                    {
                        id = l.Id,
                        text = l.Text,
                        latitude = l.Latitude,
                        longitude = l.Longitude,
                        labelType = l.LabelType,
                        color = l.Color,
                        icon = l.Icon,
                        fontSize = l.FontSize,
                        fontWeight = l.FontWeight,
                        properties = l.Properties,
                        description = l.Description,
                        lastModified = l.LastModified
                    })
                    .ToListAsync();

                return Json(new { success = true, data = labels });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labels");
                return Json(new { success = false, message = "Error retrieving labels" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveLabel([FromBody] SaveLabelRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var label = new MapLabel
                {
                    Text = request.Text,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    LabelType = request.LabelType,
                    Color = request.Color,
                    Icon = request.Icon,
                    FontSize = request.FontSize,
                    FontWeight = request.FontWeight,
                    Properties = request.Properties,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    CreatedBy = currentUser.ApplicationUserId.ToString(),
                    TeamId = currentUser.TeamId
                };

                _context.MapLabels.Add(label);
                await _context.SaveChangesAsync();

                return Json(new { success = true, data = new { id = label.Id }, message = "Label saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving label");
                return Json(new { success = false, message = "Error saving label" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLabel(Guid id)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var label = await _context.MapLabels.FindAsync(id);
                if (label == null)
                {
                    return Json(new { success = false, message = "Label not found" });
                }

                label.IsActive = false;
                label.UpdatedBy = currentUser.ApplicationUserId.ToString();
                label.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Label deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting label");
                return Json(new { success = false, message = "Error deleting label" });
            }
        }

        #endregion

        #region Map Configuration

        [HttpGet]
        public async Task<IActionResult> GetConfiguration(string key)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var config = await _context.MapConfigurations
                    .FirstOrDefaultAsync(c => c.Key == key && c.IsActive);

                if (config == null)
                {
                    return Json(new { success = false, message = "Configuration not found" });
                }

                return Json(new { success = true, data = new { value = config.Value, properties = config.Properties } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration");
                return Json(new { success = false, message = "Error retrieving configuration" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfiguration([FromBody] SaveConfigurationRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var existingConfig = await _context.MapConfigurations
                    .FirstOrDefaultAsync(c => c.Key == request.Key && c.IsActive);

                if (existingConfig != null)
                {
                    existingConfig.Value = request.Value;
                    existingConfig.Properties = request.Properties;
                    existingConfig.LastModified = DateTime.Now;
                    existingConfig.UpdatedBy = currentUser.ApplicationUserId.ToString();
                }
                else
                {
                    var config = new MapConfiguration
                    {
                        ConfigurationType = request.ConfigurationType,
                        Key = request.Key,
                        Value = request.Value,
                        Properties = request.Properties,
                        Description = request.Description,
                        LastModified = DateTime.Now,
                        CreatedBy = currentUser.ApplicationUserId.ToString(),
                        TeamId = currentUser.TeamId
                    };

                    _context.MapConfigurations.Add(config);
                }

                await _context.SaveChangesAsync();

                // Also persist map defaults to appsettings.json for offline consumers
                if (!string.IsNullOrWhiteSpace(request.Key) &&
                    (request.Key.Equals("DefaultMapPath", StringComparison.OrdinalIgnoreCase) ||
                     request.Key.Equals("DefaultSatelliteMapPath", StringComparison.OrdinalIgnoreCase) ||
                     request.Key.Equals("DefaultTerrainDbPath", StringComparison.OrdinalIgnoreCase) ||
                     request.Key.Equals("TileServerMapIdStreet", StringComparison.OrdinalIgnoreCase) ||
                     request.Key.Equals("TileServerMapIdSatellite", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var appSettingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
                        if (System.IO.File.Exists(appSettingsPath))
                        {
                            var text = await System.IO.File.ReadAllTextAsync(appSettingsPath);

                            // Replace existing key inside MapSettings
                            var keyName = request.Key;
                            var pattern = $"(\\\"{Regex.Escape(keyName)}\\\"\\s*:\\s*)\\\"[^\\\"]*\\\"";
                            if (Regex.IsMatch(text, pattern))
                            {
                                text = Regex.Replace(text, pattern, m =>
                                {
                                    var prefix = m.Groups[1].Value;
                                    var value = request.Value ?? string.Empty;
                                    return prefix + "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                                });
                            }
                            else
                            {
                                // Insert key into MapSettings block if missing
                                var mapSettingsBlock = Regex.Match(text, "\\\"MapSettings\\\"\\s*:\\s*\\{[\\s\\S]*?\\}");
                                if (mapSettingsBlock.Success)
                                {
                                    var block = mapSettingsBlock.Value;
                                    // Determine indentation
                                    var indentMatch = Regex.Match(block, "\\n(\\s*)\\\"MapsDirectory\\\"|");
                                    var indent = indentMatch.Success ? indentMatch.Groups[1].Value : "    ";
                                    var insertion = $"\n{indent}\"{keyName}\": \"{request.Value}\",";
                                    // Insert after opening brace of MapSettings
                                    var updatedBlock = new Regex("\\{").Replace(block, "{" + insertion, 1);
                                    text = text.Replace(block, updatedBlock);
                                }
                            }

                            await System.IO.File.WriteAllTextAsync(appSettingsPath, text);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to persist DefaultMapPath to appsettings.json");
                    }
                }

                return Json(new { success = true, message = "Configuration saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                return Json(new { success = false, message = "Error saving configuration" });
            }
        }

        #endregion
    }

    #region Request DTOs

    public class SaveRegionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public string? Properties { get; set; }
        public decimal? AreaM2 { get; set; }
        public decimal? CenterLat { get; set; }
        public decimal? CenterLng { get; set; }
        public string? RegionType { get; set; }
        public string? Description { get; set; }
        public bool IsLocked { get; set; }
    }

    public class UpdateRegionRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public string? Properties { get; set; }
        public decimal? AreaM2 { get; set; }
        public decimal? CenterLat { get; set; }
        public decimal? CenterLng { get; set; }
        public string? RegionType { get; set; }
        public string? Description { get; set; }
        public bool IsLocked { get; set; }
    }

    public class SaveSectorRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public string LandType { get; set; } = string.Empty;
        public string? Properties { get; set; }
        public decimal? AreaM2 { get; set; }
        public decimal? CenterLat { get; set; }
        public decimal? CenterLng { get; set; }
        public string? SectorType { get; set; }
        public string? Description { get; set; }
    }

    public class SaveLabelRequest
    {
        public string Text { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? LabelType { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int? FontSize { get; set; }
        public string? FontWeight { get; set; }
        public string? Properties { get; set; }
        public string? Description { get; set; }
    }

    public class SaveConfigurationRequest
    {
        public string ConfigurationType { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Properties { get; set; }
        public string? Description { get; set; }
    }

    #endregion
}

