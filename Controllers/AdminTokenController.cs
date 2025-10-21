using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.DTOs;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class AdminTokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<AdminTokenController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationUserVM user;


        public AdminTokenController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<AdminTokenController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            user = userSessionService.GetCurrentUser();
        }

        /// <summary>
        /// Display admin token management dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.TokenGroups
            .OrderBy(g => g.Name)
            .Select(g => new { g.Id, g.Name })
            .ToListAsync();
            return Json(groups);
        }


        [HttpGet]
        public async Task<IActionResult> ById([FromQuery] Guid id)
        {
            var t = await _context.Tokens.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound();
            return Json(new TokenEditDto
            {
                Id = t.Id,
                Name = t.Name,
                TokenGroupId = t.TokenGroupId,
                IsManualToken = t.IsManualToken,
                Notes = t.Notes,
                AssetImagePath = t.AssetImagePath,
                FrontCoverageKm = t.FrontCoverageKm,
                RearCoverageKm = t.RearCoverageKm,
                SideCoverageKm = t.SideCoverageKm
            });
        }



        [HttpPost]
        public async Task<IActionResult> Update([FromBody] CreateOrUpdateTokenRequest req)
        {
            if (req.Id == null) return BadRequest(new { success = false, message = "Missing token id" });
            var token = await _context.Tokens.FirstOrDefaultAsync(x => x.Id == req.Id);
            if (token == null) return NotFound(new { success = false, message = "Token not found" });


            if (!string.IsNullOrWhiteSpace(req.Name)) token.Name = req.Name.Trim();
            token.TokenGroupId = req.TokenGroupId;
            token.IsManualToken = req.IsManualToken;
            token.Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();
            token.AssetImagePath = req.AssetImagePath;
            token.FrontCoverageKm = req.FrontCoverageKm;
            token.RearCoverageKm = req.RearCoverageKm;
            token.SideCoverageKm = req.SideCoverageKm;


            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Token updated" });
        }


        [HttpPost]
        public async Task<IActionResult> ToggleActive([FromQuery] Guid id, [FromQuery] bool active)
        {
            try
            {
                _logger.LogInformation("ToggleActive called for token {TokenId} to {Active} by user {UserId}", 
                    id, active, user?.ApplicationUserId);
                
                var token = await _context.Tokens.FirstOrDefaultAsync(x => x.Id == id);
                if (token == null) 
                {
                    _logger.LogWarning("Token {TokenId} not found", id);
                    return NotFound(new { success = false, message = "Token not found" });
                }
                
                token.IsActive = active;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Token {TokenId} status updated to {Active}", id, active);
                return Json(new { success = true, message = active ? "Token activated" : "Token deactivated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleActive for token {TokenId}", id);
                return Json(new { success = false, message = "Error updating token status" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTokens()
        {
            try
            {
                _logger.LogInformation("GetTokens called by user {UserId}", user?.ApplicationUserId);
                
                var placedTokensvar = _context.Tokens
                                                .Include(t => t.TokenGroup)
                                                .OrderBy(t => t.Name)
                                                .Select(t => new TokenListItemDto
                                                {
                                                    Id = t.Id,
                                                    Name = t.Name,
                                                    TokenGroupId = t.TokenGroupId,
                                                    TokenGroupName = t.TokenGroup != null ? t.TokenGroup.Name : null,
                                                    IsActive = t.IsActive,
                                                    IsManualToken = t.IsManualToken,
                                                    LastUsed = t.LastUsed,
                                                    UsageCount = t.UsageCount,
                                                    Notes = t.Notes,
                                                    AssetImagePath = t.AssetImagePath,
                                                    FrontCoverageKm = t.FrontCoverageKm,
                    RearCoverageKm = t.RearCoverageKm,
                    SideCoverageKm = t.SideCoverageKm,
                                                    TeamId = t.TeamId,
                                                }).AsQueryable();

            if (user.TeamId != Guid.Empty && user.TeamId != null)
            {
                placedTokensvar = placedTokensvar.Where(t => t.TeamId == user.TeamId && t.IsActive);
            }
            else
            {
                placedTokensvar = placedTokensvar.Where(t => t.IsActive);
            }

                var tokens = await placedTokensvar.ToListAsync();
                
                _logger.LogInformation("Returning {Count} tokens", tokens.Count);
                return Json(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTokens");
                return Json(new { success = false, message = "Error loading tokens" });
            }
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }
        [HttpGet]
        public IActionResult Dashboard()
        {

            return View();
        }


        /// <summary>
        /// Display token group management page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManageTokenGroups(string searchTerm, string categoryFilter, bool? statusFilter)
        {
            var query = _context.TokenGroups.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.Name.Contains(searchTerm) ||
                                       g.GroupCode.Contains(searchTerm) ||
                                       (g.Description != null && g.Description.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(g => g.Category == categoryFilter);
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(g => g.IsActive == statusFilter.Value);
            }

            var tokenGroups = await query
                .OrderBy(g => g.Name)
                .Select(g => new TokenGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    GroupCode = g.GroupCode,
                    Category = g.Category,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    CreatedByUserName = g.CreatedBy,
                    CreatedAt = g.CreatedDate ?? DateTime.Now,
                    TokenCount = g.Tokens.Count(t => t.IsActive),
                    TeamAssignmentCount = g.TeamAssignments.Count(a => a.IsActive)
                })
                .ToListAsync();

            var availableCategories = await _context.TokenGroups
                .Where(g => !string.IsNullOrEmpty(g.Category))
                .Select(g => g.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var viewModel = new TokenGroupIndexViewModel
            {
                TokenGroups = tokenGroups,
                SearchTerm = searchTerm,
                CategoryFilter = categoryFilter,
                StatusFilter = statusFilter,
                AvailableCategories = availableCategories
            };

            return View(viewModel);
        }



        /// <summary>
        /// Display token creation page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateTokenViewModel
            {
                AvailableTokenGroups = await _context.TokenGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Name)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // ===== TOKEN GROUP CRUD METHODS =====

        /// <summary>
        /// Display token group creation page
        /// </summary>
        [HttpGet]
        public IActionResult CreateTokenGroup()
        {
            return View(new CreateTokenGroupViewModel());
        }

        /// <summary>
        /// Create a new token group
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTokenGroup(CreateTokenGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    ModelState.AddModelError("", "User not authenticated");
                    return View(model);
                }

                // Check if group code already exists
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == model.GroupCode);

                if (existingGroup != null)
                {
                    ModelState.AddModelError(nameof(model.GroupCode), "A token group with this code already exists");
                    return View(model);
                }

                var tokenGroup = new TokenGroup
                {
                    Name = model.Name,
                    GroupCode = model.GroupCode,
                    Category = model.Category,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedBy = currentUser.FullName
                };

                _context.TokenGroups.Add(tokenGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created token group: {GroupName} ({GroupCode}) by user {UserId}",
                    model.Name, model.GroupCode, currentUser.ApplicationUserId);

                TempData["SuccessMessage"] = "Token group created successfully!";
                return RedirectToAction("ManageTokenGroups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token group");
                ModelState.AddModelError("", "An error occurred while creating the token group. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Display token group details
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TokenGroupDetails(Guid id)
        {
            var tokenGroup = await _context.TokenGroups
                .Include(g => g.Tokens.Where(t => t.IsActive))
                .Include(g => g.TeamAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.Team)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (tokenGroup == null)
            {
                return NotFound();
            }

            var viewModel = new TokenGroupDetailsViewModel
            {
                Id = tokenGroup.Id,
                Name = tokenGroup.Name,
                GroupCode = tokenGroup.GroupCode,
                Category = tokenGroup.Category,
                Description = tokenGroup.Description,
                IsActive = tokenGroup.IsActive,
                CreatedByUserName = tokenGroup.CreatedBy,
                CreatedAt = tokenGroup.CreatedDate ?? DateTime.Now,
                TokenCount = tokenGroup.Tokens.Count,
                TeamAssignmentCount = tokenGroup.TeamAssignments.Count,
                Tokens = tokenGroup.Tokens.ToList(),
                TeamAssignments = tokenGroup.TeamAssignments.ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Display token group edit page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditTokenGroup(Guid id)
        {
            var tokenGroup = await _context.TokenGroups.FindAsync(id);
            if (tokenGroup == null)
            {
                return NotFound();
            }

            var viewModel = new EditTokenGroupViewModel
            {
                Id = tokenGroup.Id,
                Name = tokenGroup.Name,
                GroupCode = tokenGroup.GroupCode,
                Category = tokenGroup.Category,
                Description = tokenGroup.Description,
                IsActive = tokenGroup.IsActive
            };

            return View(viewModel);
        }

        /// <summary>
        /// Update an existing token group
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTokenGroup(EditTokenGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var tokenGroup = await _context.TokenGroups.FindAsync(model.Id);
                if (tokenGroup == null)
                {
                    return NotFound();
                }

                // Check if group code already exists (excluding current group)
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == model.GroupCode && g.Id != model.Id);

                if (existingGroup != null)
                {
                    ModelState.AddModelError(nameof(model.GroupCode), "A token group with this code already exists");
                    return View(model);
                }

                tokenGroup.Name = model.Name;
                tokenGroup.GroupCode = model.GroupCode;
                tokenGroup.Category = model.Category;
                tokenGroup.Description = model.Description;
                tokenGroup.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated token group: {GroupName} ({GroupCode})",
                    model.Name, model.GroupCode);

                TempData["SuccessMessage"] = "Token group updated successfully!";
                return RedirectToAction("ManageTokenGroups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token group");
                ModelState.AddModelError("", "An error occurred while updating the token group. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Delete a token group
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTokenGroup(Guid id)
        {
            try
            {
                var tokenGroup = await _context.TokenGroups.FindAsync(id);
                if (tokenGroup == null)
                {
                    return NotFound();
                }

                // Check if group has active tokens
                var hasActiveTokens = await _context.Tokens
                    .AnyAsync(t => t.TokenGroupId == id && t.IsActive);

                if (hasActiveTokens)
                {
                    TempData["ErrorMessage"] = "Cannot delete token group with active tokens. Please reassign or deactivate tokens first.";
                    return RedirectToAction("ManageTokenGroups");
                }

                // Soft delete by setting IsActive to false
                tokenGroup.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted token group: {GroupName} ({GroupCode})",
                    tokenGroup.Name, tokenGroup.GroupCode);

                TempData["SuccessMessage"] = "Token group deleted successfully!";
                return RedirectToAction("ManageTokenGroups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token group");
                TempData["ErrorMessage"] = "An error occurred while deleting the token group. Please try again.";
                return RedirectToAction("ManageTokenGroups");
            }
        }

        /// <summary>
        /// Get all token groups for admin management
        /// </summary>
        [HttpGet("groups")]
        public async Task<IActionResult> GetTokenGroups()
        {
            try
            {
                var groups = await _context.TokenGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Name)
                    .Select(g => new
                    {
                        id = g.Id,
                        name = g.Name,
                        description = g.Description,
                        groupCode = g.GroupCode,
                        category = g.Category,
                        isActive = g.IsActive,
                        createdAt = g.CreatedDate ?? DateTime.Now,
                        createdByUserName = g.CreatedBy
                    })
                    .ToListAsync();

                return Json(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token groups");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a token (without physical characteristics)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTokenViewModel model)
        {
            // Reload token groups for the view in case of validation errors
            model.AvailableTokenGroups = await _context.TokenGroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                //// Handle image upload if provided
                //string? imagePath = null;
                //if (model.AssetImage != null && model.AssetImage.Length > 0)
                //{
                //    imagePath = await UploadAssetImage(model.AssetImage, model.Name);
                //}

                var token = new Token
                {
                    Name = model.Name,
                    TokenGroupId = model.TokenGroupId,
                    IsManualToken = true,
                    IsActive = true,
                    AssetImagePath = "",
                    FrontCoverageKm = model.FrontCoverageKm,
                    RearCoverageKm = model.RearCoverageKm,
                    SideCoverageKm = model.SideCoverageKm,
                    // Military Unit Classification
                    OrganizationLevel = model.OrganizationLevel,
                    UnitType = model.UnitType,
                    UnitDesignation = model.UnitDesignation,
                    ForceType = model.ForceType
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                // Create initial area coverage if position and coverage values are provided
                if (model.CurrentLatitude.HasValue && model.CurrentLongitude.HasValue)
                {
                    // Create oval coverage if front/rear values are specified
                    if (model.FrontCoverageKm.HasValue && model.RearCoverageKm.HasValue)
                    {
                        await CreateInitialOvalAreaCoverage(token.Id, model.CurrentLatitude.Value,
                            model.CurrentLongitude.Value, model.FrontCoverageKm.Value, 
                            model.RearCoverageKm.Value, model.SideCoverageKm);
                    }
                }


                TempData["SuccessMessage"] = "Token created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token");
                ModelState.AddModelError("", "An error occurred while creating the token. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Edit token GET action
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (token == null)
                {
                    TempData["ErrorMessage"] = "Token not found.";
                    return RedirectToAction("Index");
                }

                var availableTokenGroups = await _context.TokenGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Name)
                    .ToListAsync();

                var model = new CreateTokenViewModel
                {
                    Name = token.Name,
                    TokenGroupId = token.TokenGroupId,
                    FrontCoverageKm = token.FrontCoverageKm,
                    RearCoverageKm = token.RearCoverageKm,
                    SideCoverageKm = token.SideCoverageKm,
                    // Military Unit Classification
                    OrganizationLevel = token.OrganizationLevel,
                    UnitType = token.UnitType,
                    UnitDesignation = token.UnitDesignation,
                    ForceType = token.ForceType,
                    AvailableTokenGroups = availableTokenGroups,
                    IsEdit = true
                };

                ViewData["TokenId"] = id;
                ViewData["CurrentImagePath"] = token.AssetImagePath;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading token for edit: {TokenId}", id);
                TempData["ErrorMessage"] = "Error loading token for editing.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Edit token POST action
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateTokenViewModel model)
        {
            model.AvailableTokenGroups = await _context.TokenGroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewData["TokenId"] = id;
                return View(model);
            }

            try
            {
                var token = await _context.Tokens.FirstOrDefaultAsync(t => t.Id == id);
                if (token == null)
                {
                    TempData["ErrorMessage"] = "Token not found.";
                    return RedirectToAction("Index");
                }

                //// Handle image upload if new image provided
                //string? imagePath = token.AssetImagePath; // Keep existing image by default
                //if (model.AssetImage != null && model.AssetImage.Length > 0)
                //{
                //    // Delete old image if it exists
                //    if (!string.IsNullOrEmpty(token.AssetImagePath))
                //    {
                //        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, token.AssetImagePath.TrimStart('/'));
                //        if (System.IO.File.Exists(oldImagePath))
                //        {
                //            System.IO.File.Delete(oldImagePath);
                //        }
                //    }

                //    imagePath = await UploadAssetImage(model.AssetImage, model.Name);
                //}

                // Update token properties
                token.Name = model.Name;
                token.TokenGroupId = model.TokenGroupId;
                token.AssetImagePath = "";
                token.FrontCoverageKm = model.FrontCoverageKm;
                token.RearCoverageKm = model.RearCoverageKm;
                token.SideCoverageKm = model.SideCoverageKm;
                
                // Update military classification
                token.OrganizationLevel = model.OrganizationLevel;
                token.UnitType = model.UnitType;
                token.UnitDesignation = model.UnitDesignation;
                token.ForceType = model.ForceType;

                // Update position if provided
                var positionChanged = model.CurrentLatitude.HasValue && model.CurrentLongitude.HasValue;

                _context.Update(token);
                await _context.SaveChangesAsync();

                // Update coverage area if position or coverage changed
                if (positionChanged && model.CurrentLatitude.HasValue &&
                    model.CurrentLongitude.HasValue && model.FrontCoverageKm.HasValue && model.RearCoverageKm.HasValue)
                {
                    await UpdateTokenOvalCoverageArea(token.Id, model.CurrentLatitude.Value,
                        model.CurrentLongitude.Value, model.FrontCoverageKm.Value, 
                        model.RearCoverageKm.Value, model.SideCoverageKm);
                }

                TempData["SuccessMessage"] = "Token updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token: {TokenId}", id);
                ModelState.AddModelError("", "An error occurred while updating the token. Please try again.");
                ViewData["TokenId"] = id;
                return View(model);
            }
        }

        /// <summary>
        /// Get all teams for admin management
        /// </summary>
        [HttpGet("teams")]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var teams = await _context.Teams
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        teamCode = t.TeamCode,
                        subTeamCode = t.SubTeamCode,
                        description = t.Description,
                        isActive = t.IsActive,
                        createdAt = t.CreatedDate ?? DateTime.Now
                    })
                    .ToListAsync();

                return Json(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teams");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload asset image
        /// </summary>
        private async Task<string> UploadAssetImage(IFormFile imageFile, string assetName)
        {
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "asset-images");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}_{assetName.Replace(" ", "_")}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/uploads/asset-images/{fileName}";
        }

        /// <summary>
        /// Create initial area coverage for token (simple version without asset type)
        /// </summary>
        private async Task CreateInitialAreaCoverage(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm)
        {
            var geometry = CreateCircleGeometry(latitude, longitude, radiusKm);

            var areaCoverage = new TokenAreaCoverage
            {
                TokenId = tokenId,
                Name = "Operational Area",
                Geometry = System.Text.Json.JsonSerializer.Serialize(geometry),
                AreaKm2 = (decimal)(Math.PI * Math.Pow((double)radiusKm, 2)),
                CoverageType = "Operational",
                ShapeType = "Circle",
                IsActive = true,
                IsDynamic = true,
                Description = "Initial operational area for token"
            };

            _context.TokenAreaCoverages.Add(areaCoverage);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Create initial oval area coverage for token with front/rear/side distances
        /// </summary>
        private async Task CreateInitialOvalAreaCoverage(Guid tokenId, decimal latitude, decimal longitude, 
            decimal frontKm, decimal rearKm, decimal? sideKm = null)
        {
            // Calculate side coverage if not provided (average of front and rear)
            if (!sideKm.HasValue)
            {
                sideKm = (frontKm + rearKm) / 2;
            }

            var geometry = CreateOvalGeometry(latitude, longitude, frontKm, rearKm, sideKm.Value);

            // Calculate area for oval (approximate using ellipse formula: π * a * b)
            var semiMajorAxis = Math.Max((double)frontKm, (double)rearKm) / 2;
            var semiMinorAxis = (double)sideKm.Value / 2;
            var areaKm2 = (decimal)(Math.PI * semiMajorAxis * semiMinorAxis);

            var areaCoverage = new TokenAreaCoverage
            {
                TokenId = tokenId,
                Name = "Oval Operational Area",
                Geometry = System.Text.Json.JsonSerializer.Serialize(geometry),
                AreaKm2 = areaKm2,
                FrontRadiusKm = frontKm,
                RearRadiusKm = rearKm,
                SideRadiusKm = sideKm,
                CoverageType = "Operational",
                ShapeType = "Oval",
                IsActive = true,
                IsDynamic = true,
                Description = "Initial oval operational area for token"
            };

            _context.TokenAreaCoverages.Add(areaCoverage);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update oval coverage area for a token
        /// </summary>
        private async Task UpdateTokenOvalCoverageArea(Guid tokenId, decimal latitude, decimal longitude, 
            decimal frontKm, decimal rearKm, decimal? sideKm = null)
        {
            var existingCoverages = await _context.TokenAreaCoverages
                .Where(tac => tac.TokenId == tokenId && tac.IsActive)
                .ToListAsync();

            var operationalArea = existingCoverages.FirstOrDefault(c => c.CoverageType == "Operational");

            if (operationalArea != null)
            {
                // Update existing area
                var geometry = CreateOvalGeometry(latitude, longitude, frontKm, rearKm, sideKm ?? (frontKm + rearKm) / 2);
                operationalArea.Geometry = System.Text.Json.JsonSerializer.Serialize(geometry);
                
                // Calculate area for oval (approximate using ellipse formula: π * a * b)
                var semiMajorAxis = Math.Max((double)frontKm, (double)rearKm) / 2;
                var semiMinorAxis = (double)(sideKm ?? (frontKm + rearKm) / 2) / 2;
                operationalArea.AreaKm2 = (decimal)(Math.PI * semiMajorAxis * semiMinorAxis);
                
                operationalArea.FrontRadiusKm = frontKm;
                operationalArea.RearRadiusKm = rearKm;
                operationalArea.SideRadiusKm = sideKm ?? (frontKm + rearKm) / 2;
                operationalArea.ShapeType = "Oval";
                operationalArea.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                // Create new coverage area
                await CreateInitialOvalAreaCoverage(tokenId, latitude, longitude, frontKm, rearKm, sideKm);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update token coverage area when position or radius changes (legacy method)
        /// </summary>
        private async Task UpdateTokenCoverageArea(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm)
        {
            // Find existing dynamic coverage areas
            var existingCoverages = await _context.TokenAreaCoverages
                .Where(tac => tac.TokenId == tokenId && tac.IsDynamic)
                .ToListAsync();

            // Update or create default operational area
            var operationalArea = existingCoverages.FirstOrDefault(c => c.CoverageType == "Operational");

            if (operationalArea != null)
            {
                // Update existing area
                var geometry = CreateCircleGeometry(latitude, longitude, radiusKm);
                operationalArea.Geometry = System.Text.Json.JsonSerializer.Serialize(geometry);
                operationalArea.AreaKm2 = (decimal)(Math.PI * Math.Pow((double)radiusKm, 2));
                operationalArea.FrontRadiusKm = radiusKm;
                operationalArea.RearRadiusKm = radiusKm;
                operationalArea.SideRadiusKm = radiusKm;
                operationalArea.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                // Create new coverage area
                await CreateInitialAreaCoverage(tokenId, latitude, longitude, radiusKm);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Create circle geometry for coverage area
        /// </summary>
        private object CreateCircleGeometry(decimal lat, decimal lng, decimal radiusKm)
        {
            var radiusInDegrees = radiusKm / 111.32m; // Approximate conversion
            var coordinates = GenerateCircleCoordinates(lat, lng, radiusInDegrees);

            return new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            };
        }

        /// <summary>
        /// Create oval geometry for coverage area
        /// </summary>
        private object CreateOvalGeometry(decimal lat, decimal lng, decimal frontKm, decimal rearKm, decimal sideKm)
        {
            var frontDegrees = frontKm / 111.32m; // Approximate conversion
            var rearDegrees = rearKm / 111.32m;
            var sideDegrees = sideKm / 111.32m;
            
            var coordinates = GenerateOvalCoordinates(lat, lng, frontDegrees, rearDegrees, sideDegrees);

            return new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            };
        }

        /// <summary>
        /// Generate coordinates for a circle
        /// </summary>
        private double[][] GenerateCircleCoordinates(decimal lat, decimal lng, decimal radiusDegrees)
        {
            var coordinates = new List<double[]>();
            var segments = 32;

            for (int i = 0; i <= segments; i++)
            {
                var angle = (2 * Math.PI * i) / segments;
                var x = (double)lng + (double)radiusDegrees * Math.Cos(angle);
                var y = (double)lat + (double)radiusDegrees * Math.Sin(angle);
                coordinates.Add(new double[] { x, y });
            }

            return coordinates.ToArray();
        }

        /// <summary>
        /// Generate coordinates for an oval (ellipse)
        /// </summary>
        private double[][] GenerateOvalCoordinates(decimal lat, decimal lng, decimal frontDegrees, decimal rearDegrees, decimal sideDegrees)
        {
            var coordinates = new List<double[]>();
            var segments = 32;

            // Calculate semi-major and semi-minor axes
            var semiMajorAxis = Math.Max((double)frontDegrees, (double)rearDegrees);
            var semiMinorAxis = (double)sideDegrees;

            for (int i = 0; i <= segments; i++)
            {
                var angle = (2 * Math.PI * i) / segments;
                
                // Calculate radius based on angle (front/rear direction)
                double radius;
                if (angle >= 0 && angle <= Math.PI / 2) // Front-right quadrant
                {
                    radius = (double)frontDegrees;
                }
                else if (angle > Math.PI / 2 && angle <= Math.PI) // Rear-right quadrant
                {
                    radius = (double)rearDegrees;
                }
                else if (angle > Math.PI && angle <= 3 * Math.PI / 2) // Rear-left quadrant
                {
                    radius = (double)rearDegrees;
                }
                else // Front-left quadrant
                {
                    radius = (double)frontDegrees;
                }

                // Apply elliptical scaling for side coverage
                var x = (double)lng + radius * Math.Cos(angle) * ((double)sideDegrees / semiMajorAxis);
                var y = (double)lat + radius * Math.Sin(angle);

                coordinates.Add(new double[] { x, y });
            }

            return coordinates.ToArray();
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class CreateTokenGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class CreateTokenRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
    }

    public class TokenListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
        public Guid? TeamId { get; set; }
        public string TokenGroupName { get; set; }
        public bool IsActive { get; set; }
        public bool IsManualToken { get; set; }
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; }
        public string? Notes { get; set; }
        public string? AssetImagePath { get; set; }
        public decimal? FrontCoverageKm { get; set; }
        public decimal? RearCoverageKm { get; set; }
        public decimal? SideCoverageKm { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
    }


    public class TokenEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
        public bool IsManualToken { get; set; }
        public string? Notes { get; set; }
        public string? AssetImagePath { get; set; }
        public decimal? FrontCoverageKm { get; set; }
        public decimal? RearCoverageKm { get; set; }
        public decimal? SideCoverageKm { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
    }


    public class CreateOrUpdateTokenRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
        public bool IsManualToken { get; set; }
        public string? Notes { get; set; }
        public string? AssetImagePath { get; set; }
        public decimal? FrontCoverageKm { get; set; }
        public decimal? RearCoverageKm { get; set; }
        public decimal? SideCoverageKm { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
    }
}
