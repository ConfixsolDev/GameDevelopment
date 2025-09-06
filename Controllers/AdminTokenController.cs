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
    public class AdminTokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<AdminTokenController> _logger;

        public AdminTokenController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<AdminTokenController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Display admin token management dashboard
        /// </summary>
        [HttpGet]
        public IActionResult Index()
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
                    CreatedByUserName = g.CreatedByUserName,
                    CreatedAt = g.CreatedAt,
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
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
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
                CreatedByUserName = tokenGroup.CreatedByUserName,
                CreatedAt = tokenGroup.CreatedAt,
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

        // ===== API ENDPOINTS FOR AJAX CALLS =====

        /// <summary>
        /// Create a new token group (e.g., "Company A", "Brigade 1")
        /// </summary>
        [HttpPost("create-group")]
        public async Task<IActionResult> CreateTokenGroup([FromBody] CreateTokenGroupRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Check if group code already exists
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == request.GroupCode);

                if (existingGroup != null)
                {
                    return Json(new { success = false, message = $"Token group with code '{request.GroupCode}' already exists" });
                }

                var tokenGroup = new TokenGroup
                {
                    Name = request.Name,
                    Description = request.Description,
                    GroupCode = request.GroupCode,
                    Category = request.Category,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.TokenGroups.Add(tokenGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created token group: {GroupName} ({GroupCode}) by user {UserId}", 
                    request.Name, request.GroupCode, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Token group created successfully",
                    tokenGroup = new
                    {
                        id = tokenGroup.Id,
                        name = tokenGroup.Name,
                        description = tokenGroup.Description,
                        groupCode = tokenGroup.GroupCode,
                        category = tokenGroup.Category,
                        isActive = tokenGroup.IsActive,
                        createdAt = tokenGroup.CreatedAt,
                        createdByUserName = tokenGroup.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token group");
                return Json(new { success = false, message = "Internal server error" });
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
                        createdAt = g.CreatedAt,
                        createdByUserName = g.CreatedByUserName
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
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    ModelState.AddModelError("", "User not authenticated");
                    return View(model);
                }

                // Get user details to determine team
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.ApplicationUserId);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    return View(model);
                }

                // Find the team by TeamCode and SubTeamCode
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamCode == user.TeamCode && t.SubTeamCode == user.SubTeamCode);
                if (team == null)
                {
                    ModelState.AddModelError("", "User team not found");
                    return View(model);
                }

                // Generate a unique token ID
                var tokenId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Create the token
                var token = new Token
                {
                    Id = tokenId,
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    TokenGroupId = model.TokenGroupId,
                    TeamId = team.Id,
                    IsManualToken = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created token: {TokenName} by user {UserId}", 
                    model.Name, currentUser.ApplicationUserId);

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
                        createdAt = t.CreatedAt
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
}
