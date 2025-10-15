using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.Models.AttackPlanning;
using TechWebSol.Services;
using TechWebSol.Services.TokenManagement;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// Controller for handling attack planning UI and operations
    /// </summary>
    public class AttackPlanningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttackPlanningController> _logger;
        private readonly IUserSessionService _userSessionService;
        private readonly IComprehensiveCombatSimulationService _simulationService;
        private readonly ApplicationUserVM user;

        public AttackPlanningController(
            ApplicationDbContext context, 
            ILogger<AttackPlanningController> logger,
            IUserSessionService userSessionService,
            IComprehensiveCombatSimulationService simulationService
            )
        {
            user = userSessionService.GetCurrentUser();
            _context = context;
            _logger = logger;
            _simulationService = simulationService;
        }
        /// <summary>
        /// Main attack planning dashboard
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Create new attack order form
        /// </summary>
        [HttpGet]
        public IActionResult CreateAttackOrder()
        {
            var model = new EnhancedAttackOrder();
            return PartialView("_AttackPlanningModal", model);
        }

        /// <summary>
        /// Load attack intent form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadIntentForm()
        {
            var model = new AttackIntent();
            return PartialView("Partials/_AttackIntentForm", model);
        }

        /// <summary>
        /// Load attack timing form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadTimingForm()
        {
            var model = new AttackTiming();
            return PartialView("Partials/_AttackTimingForm", model);
        }

        /// <summary>
        /// Load attack movement form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadMovementForm()
        {
            var model = new AttackMovement();
            return PartialView("Partials/_AttackMovementForm", model);
        }

        /// <summary>
        /// Load fires support form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadFiresForm()
        {
            var model = new FiresSupport();
            return PartialView("Partials/_FiresSupportForm", model);
        }

        /// <summary>
        /// Load fog of war form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadFogOfWarForm()
        {
            var model = new FogOfWar();
            return PartialView("Partials/_FogOfWarForm", model);
        }

        /// <summary>
        /// Load logistics form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadLogisticsForm()
        {
            var model = new AttackLogistics();
            return PartialView("Partials/_AttackLogisticsForm", model);
        }

        /// <summary>
        /// Load ROE form tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadROEForm()
        {
            var model = new RulesOfEngagement();
            return PartialView("Partials/_RulesOfEngagementForm", model);
        }

        /// <summary>
        /// Load attack summary tab
        /// </summary>
        [HttpGet]
        public IActionResult LoadSummaryForm()
        {
            var model = new EnhancedAttackOrder();
            return PartialView("Partials/_SummaryForm", model);
        }

        /// <summary>
        /// Check if attack order exists for token pair
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckExistingAttackOrder(Guid attackerTokenId, Guid targetTokenId)
        {
            try
            {
                var existingOrder = await _context.EnhancedAttackOrders
                    .FirstOrDefaultAsync(o => o.AttackerTokenId == attackerTokenId && o.TargetTokenId == targetTokenId);

                if (existingOrder != null)
                {
                    return Json(new { 
                        success = true, 
                        exists = true, 
                        orderId = existingOrder.Id.ToString(),
                        createdDate = existingOrder.CreatedDate,
                        updatedDate = existingOrder.UpdatedDate,
                        data = new
                        {
                            intent = existingOrder.IntentJson,
                            timing = existingOrder.TimingJson,
                            movement = existingOrder.MovementJson,
                            fires = existingOrder.FiresJson,
                            fogOfWar = existingOrder.FogOfWarJson,
                            logistics = existingOrder.LogisticsJson,
                            roe = existingOrder.ROEJson
                        }
                    });
                }

                return Json(new { success = true, exists = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existing attack order");
                return Json(new { success = false, message = "Failed to check existing attack order" });
            }
        }

        /// <summary>
        /// Get all attack orders for visualization
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAttackOrders()
        {
            try
            {
                var attackorderlist =  _context.EnhancedAttackOrders
                    .Where(o => o.AttackerTokenId != Guid.Empty && o.TargetTokenId != Guid.Empty )
                    .Select(o => new
                    {
                        Id = o.Id,
                        AttackerTokenId = o.AttackerTokenId,
                        TargetTokenId = o.TargetTokenId,
                        IntentJson = o.IntentJson,
                        TimingJson = o.TimingJson,
                        MovementJson = o.MovementJson,
                        FiresJson = o.FiresJson,
                        FogOfWarJson = o.FogOfWarJson,
                        LogisticsJson = o.LogisticsJson,
                        ROEJson = o.ROEJson,
                        CreatedDate = o.CreatedDate,
                        UpdatedDate = o.UpdatedDate,
                        TeamId = o.TeamId
                    })
                    .AsQueryable();

                if (user.TeamId != Guid.Empty)
                {
                    attackorderlist = attackorderlist.Where(x => x.TeamId == user.TeamId).AsQueryable();
                }
                var attackOrders = await attackorderlist.ToListAsync();
                return Json(new { success = true, attackOrders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attack orders");
                return Json(new { success = false, message = "Failed to get attack orders" });
            }
        }

        /// <summary>
        /// Update an attack order's token IDs
        /// </summary>
    [HttpDelete]
    [Route("DeleteAttackOrder/{orderId}")]
    public async Task<IActionResult> DeleteAttackOrder(string orderId)
    {
        try
        {
            var order = await _context.EnhancedAttackOrders.FindAsync(Guid.Parse(orderId));
            if (order == null)
            {
                return Json(new { success = false, message = "Attack order not found" });
            }

            _context.EnhancedAttackOrders.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Attack order deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attack order");
            return Json(new { success = false, message = "Failed to delete attack order" });
        }
    }

        /// <summary>
        /// Delete an attack order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAttackOrder([FromBody] DeleteAttackOrderRequest request)
        {
            try
            {
                var order = await _context.EnhancedAttackOrders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order != null)
                {
                    _context.EnhancedAttackOrders.Remove(order);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Attack order deleted successfully" });
                }

                return Json(new { success = false, message = "Attack order not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attack order");
                return Json(new { success = false, message = "Failed to delete attack order" });
            }
        }

        /// <summary>
        /// Save draft attack order (auto-save or manual save)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request)
        {
            try
            {
                // Check if order already exists
                var existingOrder = await _context.EnhancedAttackOrders.FirstOrDefaultAsync(o => o.Id.ToString() == request.OrderId || o.AttackerTokenId == request.AttackerTokenId && o.TargetTokenId == request.TargetTokenId);

                if (existingOrder == null)
                {
                    // Create new order with attacker and target token IDs
                    existingOrder = new EnhancedAttackOrder
                    {
                        AttackerTokenId = request.AttackerTokenId ?? Guid.Empty,
                        TargetTokenId = request.TargetTokenId ?? Guid.Empty,
                    };
                    _context.EnhancedAttackOrders.Add(existingOrder);
                } 
               
                // Update specific tab data based on TabName
                switch (request.TabName.ToLower())
                {
                    case "intent":
                        var intent = JsonSerializer.Deserialize<AttackIntent>(existingOrder.IntentJson ?? "{}") ?? new AttackIntent();
                        UpdateIntentFromData(intent, request.Data);
                        existingOrder.IntentJson = JsonSerializer.Serialize(intent);
                        break;
                    case "timing":
                        var timing = JsonSerializer.Deserialize<AttackTiming>(existingOrder.TimingJson ?? "{}") ?? new AttackTiming();
                        UpdateTimingFromData(timing, request.Data);
                        existingOrder.TimingJson = JsonSerializer.Serialize(timing);
                        break;
                    case "movement":
                        var movement = JsonSerializer.Deserialize<AttackMovement>(existingOrder.MovementJson ?? "{}") ?? new AttackMovement();
                        UpdateMovementFromData(movement, request.Data);
                        existingOrder.MovementJson = JsonSerializer.Serialize(movement);
                        break;
                    case "fires":
                        var fires = JsonSerializer.Deserialize<FiresSupport>(existingOrder.FiresJson ?? "{}") ?? new FiresSupport();
                        UpdateFiresFromData(fires, request.Data);
                        existingOrder.FiresJson = JsonSerializer.Serialize(fires);
                        break;
                    case "fogofwar":
                        var fogOfWar = JsonSerializer.Deserialize<FogOfWar>(existingOrder.FogOfWarJson ?? "{}") ?? new FogOfWar();
                        UpdateFogOfWarFromData(fogOfWar, request.Data);
                        existingOrder.FogOfWarJson = JsonSerializer.Serialize(fogOfWar);
                        break;
                    case "logistics":
                        var logistics = JsonSerializer.Deserialize<AttackLogistics>(existingOrder.LogisticsJson ?? "{}") ?? new AttackLogistics();
                        UpdateLogisticsFromData(logistics, request.Data);
                        existingOrder.LogisticsJson = JsonSerializer.Serialize(logistics);
                        break;
                    case "roe":
                        var roe = JsonSerializer.Deserialize<RulesOfEngagement>(existingOrder.ROEJson ?? "{}") ?? new RulesOfEngagement();
                        UpdateRoeFromData(roe, request.Data);
                        existingOrder.ROEJson = JsonSerializer.Serialize(roe);
                        break;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Draft saved successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving draft: {ex.Message}");
                return Json(new { success = false, message = "Failed to save draft" });
            }
        }

        /// <summary>
        /// Validate attack order
        /// </summary>
        [HttpPost]
        public IActionResult ValidateOrder([FromBody] EnhancedAttackOrder order)
        {
            try
            {
                var validationResults = new List<string>();

                // Basic validation
                if (order.AttackerTokenId == Guid.Empty)
                    validationResults.Add("Attacker token ID is required");

                if (order.TargetTokenId == Guid.Empty)
                    validationResults.Add("Target token ID is required");

                if (order.AttackerTokenId == order.TargetTokenId)
                    validationResults.Add("Attacker and target cannot be the same");

                // TODO: Add more validation logic

                return Json(new { 
                    success = validationResults.Count == 0, 
                    errors = validationResults,
                    isValid = validationResults.Count == 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating order: {ex.Message}");
                return Json(new { success = false, message = "Validation failed" });
            }
        }

        /// <summary>
        /// Preview attack order
        /// </summary>
        [HttpPost]
        public IActionResult PreviewAttack([FromBody] EnhancedAttackOrder order)
        {
            try
            {
                // TODO: Implement attack preview logic
                Console.WriteLine($"Generating preview for attack order: {order.Id}");

                return Json(new
                {
                    success = true,
                    preview = new
                    {
                        attackPreparation = order.Intent?.AttackPreparation,
                        natoAttackType = order.Intent?.NatoAttackType,
                        attackIntensity = order.Intent?.AttackIntensity,
                        coordinationType = order.Intent?.CoordinationType,
                        startTurn = order.Timing?.StartTurn,
                        duration = order.Timing?.DurationTurns,
                        confidence = order.FogOfWar?.DetectionConfidence
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating preview: {ex.Message}");
                return Json(new { success = false, message = "Preview generation failed" });
            }
        }
        #region Helper Methods for Data Updates

        private void UpdateIntentFromData(AttackIntent intent, Dictionary<string, object> data)
        {
            if (data.ContainsKey("AttackPreparation"))
                intent.AttackPreparation = data["AttackPreparation"]?.ToString();
            if (data.ContainsKey("NatoAttackType"))
                intent.NatoAttackType = data["NatoAttackType"]?.ToString();
            if (data.ContainsKey("AttackIntensity"))
                intent.AttackIntensity = data["AttackIntensity"]?.ToString();
            if (data.ContainsKey("CoordinationType"))
                intent.CoordinationType = data["CoordinationType"]?.ToString();
            if (data.ContainsKey("DesiredEffect"))
                intent.DesiredEffect = data["DesiredEffect"]?.ToString();
            if (data.ContainsKey("Notes"))
                intent.Notes = data["Notes"]?.ToString();
        }

        private void UpdateTimingFromData(AttackTiming timing, Dictionary<string, object> data)
        {
            if (data.ContainsKey("StartTurn") && int.TryParse(data["StartTurn"]?.ToString(), out int startTurn))
                timing.StartTurn = startTurn;
            if (data.ContainsKey("DurationTurns") && int.TryParse(data["DurationTurns"]?.ToString(), out int duration))
                timing.DurationTurns = duration;
            if (data.ContainsKey("Posture"))
                timing.Posture = data["Posture"]?.ToString();
            if (data.ContainsKey("Notes"))
                timing.Notes = data["Notes"]?.ToString();
        }

        private void UpdateMovementFromData(AttackMovement movement, Dictionary<string, object> data)
        {
            if (data.ContainsKey("MpReservePercent") && decimal.TryParse(data["MpReservePercent"]?.ToString(), out decimal reserve))
                movement.MpReservePercent = reserve;
            if (data.ContainsKey("Notes"))
                movement.Notes = data["Notes"]?.ToString();
            // TODO: Handle waypoints JSON data
        }

        private void UpdateFiresFromData(FiresSupport fires, Dictionary<string, object> data)
        {
            if (data.ContainsKey("ArtilleryTask"))
                fires.ArtilleryTask = data["ArtilleryTask"]?.ToString();
            if (data.ContainsKey("EngineersPresent") && bool.TryParse(data["EngineersPresent"]?.ToString(), out bool engineers))
                fires.EngineersPresent = engineers;
            if (data.ContainsKey("Notes"))
                fires.Notes = data["Notes"]?.ToString();
            // TODO: Handle artillery attached JSON data
        }

        private void UpdateFogOfWarFromData(FogOfWar fogOfWar, Dictionary<string, object> data)
        {
            if (data.ContainsKey("DetectionConfidence") && decimal.TryParse(data["DetectionConfidence"]?.ToString(), out decimal confidence))
                fogOfWar.DetectionConfidence = confidence;
            if (data.ContainsKey("CommitWithUncertainty") && bool.TryParse(data["CommitWithUncertainty"]?.ToString(), out bool commit))
                fogOfWar.CommitWithUncertainty = commit;
            if (data.ContainsKey("Notes"))
                fogOfWar.Notes = data["Notes"]?.ToString();
            // TODO: Handle abort criteria JSON data
        }

        private void UpdateLogisticsFromData(AttackLogistics logistics, Dictionary<string, object> data)
        {
            if (data.ContainsKey("SupplyThreshold"))
                logistics.SupplyThreshold = data["SupplyThreshold"]?.ToString();
            if (data.ContainsKey("Notes"))
                logistics.Notes = data["Notes"]?.ToString();
        }

        private void UpdateRoeFromData(RulesOfEngagement roe, Dictionary<string, object> data)
        {
            if (data.ContainsKey("CollateralSensitivity"))
                roe.CollateralSensitivity = data["CollateralSensitivity"]?.ToString();
            if (data.ContainsKey("Notes"))
                roe.Notes = data["Notes"]?.ToString();
        }
        #endregion

        /// <summary>
        /// Get attack order selection modal as partial view
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttackOrderSelectionModal()
        {
            try
            {
                var attackOrders = await _context.EnhancedAttackOrders
                    .Where(ao => ao.IsActive && ao.TeamId == user.TeamId)
                    .OrderByDescending(ao => ao.CreatedDate)
                    .ToListAsync();

                var ordersWithDetails = new List<AttackOrderSelectionViewModel>();

                foreach (var order in attackOrders)
                {
                    var attackerToken = await _context.Tokens
                        .Where(t => t.Id == order.AttackerTokenId)
                        .Select(t => new { t.Name, t.UnitDesignation })
                        .FirstOrDefaultAsync();

                    var realTargetToken = await _context.Tokens
                        .Where(t => t.Id == order.TargetTokenId)
                        .Select(t => new { t.Name, t.UnitDesignation })
                        .FirstOrDefaultAsync();

                    var suspectedTargetToken = await _context.SuspectedTokens
                        .Where(st => st.Id == order.TargetTokenId)
                        .Select(st => new { st.Name, st.RealTokenId })
                        .FirstOrDefaultAsync();

                    bool isSuspectedToken = suspectedTargetToken != null;
                    string targetName = realTargetToken?.Name ?? suspectedTargetToken?.Name ?? "Unknown";

                    ordersWithDetails.Add(new AttackOrderSelectionViewModel
                    {
                        Id = order.Id,
                        AttackerTokenId = order.AttackerTokenId,
                        TargetTokenId = order.TargetTokenId,
                        AttackerTokenName = attackerToken?.Name ?? attackerToken?.UnitDesignation ?? "Unknown",
                        TargetTokenName = targetName,
                        IsSuspectedToken = isSuspectedToken,
                        Status = order.Status,
                        CompletionPercentage = order.CompletionPercentage,
                        LastUpdated = order.LastUpdated
                    });
                }

                return PartialView("Partials/_AttackOrderSelectionModal", ordersWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attack orders");
                return Content($"<div class='alert alert-danger'>Error: {ex.Message}</div>");
            }
        }

        /// <summary>
        /// Get all available attack orders for simulation (summary only) - JSON API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAvailableAttackOrders()
        {
            try
            {
                var attackOrders = await _context.EnhancedAttackOrders
                    .Where(ao => ao.IsActive && ao.TeamId == user.TeamId)
                    .OrderByDescending(ao => ao.CreatedDate)
                    .ToListAsync();

                var ordersWithDetails = new List<object>();

                foreach (var order in attackOrders)
                {
                    // Get attacker token details
                    var attackerToken = await _context.Tokens
                        .Where(t => t.Id == order.AttackerTokenId)
                        .Select(t => new { t.Name, t.UnitDesignation })
                        .FirstOrDefaultAsync();

                    // Check if target is a suspected token or real token
                    var realTargetToken = await _context.Tokens
                        .Where(t => t.Id == order.TargetTokenId)
                        .Select(t => new { t.Name, t.UnitDesignation })
                        .FirstOrDefaultAsync();

                    var suspectedTargetToken = await _context.SuspectedTokens
                        .Where(st => st.Id == order.TargetTokenId)
                        .Select(st => new { st.Name, st.RealTokenId })
                        .FirstOrDefaultAsync();

                    bool isSuspectedToken = suspectedTargetToken != null;
                    string targetName = realTargetToken?.Name ?? suspectedTargetToken?.Name ?? "Unknown";

                    ordersWithDetails.Add(new
                    {
                        id = order.Id,
                        attackerTokenId = order.AttackerTokenId,
                        targetTokenId = order.TargetTokenId,
                        attackerTokenName = attackerToken?.Name ?? attackerToken?.UnitDesignation ?? "Unknown",
                        targetTokenName = targetName,
                        isSuspectedToken = isSuspectedToken,
                        status = order.Status,
                        completionPercentage = order.CompletionPercentage,
                        lastUpdated = order.LastUpdated
                    });
                }

                return Json(new
                {
                    success = true,
                    attackOrders = ordersWithDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attack orders");
                return Json(new { success = false, message = "Error fetching attack orders: " + ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive attack and defense data for simulation
        /// Returns ALL information about attacker, defender, and battlefield in a single call
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComprehensiveAttackDefenseData(string attackOrderId)
        {
            try
            {
                if (!Guid.TryParse(attackOrderId, out var orderGuid))
                {
                    return Json(new { success = false, message = "Invalid attack order ID" });
                }

                // Get attack order
                var attackOrder = await _context.EnhancedAttackOrders
                    .FirstOrDefaultAsync(ao => ao.Id == orderGuid && ao.IsActive);

                if (attackOrder == null)
                {
                    return Json(new { success = false, message = "Attack order not found" });
                }

                // ===== ATTACKER DATA =====
                var attackerToken = await _context.Tokens
                    .Include(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(t => t.Id == attackOrder.AttackerTokenId);

                // UnitDeployment removed - using Brigade system now
                // var attackerDeployment = await _context.UnitDeployments...

                var attackerLocation = attackerToken?.MapMarkers?.FirstOrDefault();

                // ===== DEFENDER DATA =====
                // Check if target is suspected token or real token
                var suspectedTarget = await _context.SuspectedTokens
                    .Include(st => st.RealToken)
                        .ThenInclude(t => t.MapMarkers.Where(m => m.IsActive))
                    .FirstOrDefaultAsync(st => st.Id == attackOrder.TargetTokenId);

                Token? defenderToken = null;
                MapMarker? defenderLocation = null;
                bool isSuspectedToken = false;
                decimal detectionConfidence = 100m;

                if (suspectedTarget != null)
                {
                    // Target is suspected token - resolve to real token
                    isSuspectedToken = true;
                    detectionConfidence = suspectedTarget.Confidence;
                    
                    if (suspectedTarget.RealTokenId.HasValue)
                    {
                        defenderToken = suspectedTarget.RealToken;
                    }
                    
                    // Use suspected location
                    defenderLocation = new MapMarker
                    {
                        latitude = suspectedTarget.Latitude.ToString(),
                        longitude = suspectedTarget.Longitude.ToString(),
                        Position = $"{{\"lat\":{suspectedTarget.Latitude},\"lng\":{suspectedTarget.Longitude}}}"
                    };
                }
                else
                {
                    // Target is real token
                    defenderToken = await _context.Tokens
                        .Include(t => t.MapMarkers.Where(m => m.IsActive))
                        .FirstOrDefaultAsync(t => t.Id == attackOrder.TargetTokenId);
                    
                    defenderLocation = defenderToken?.MapMarkers?.FirstOrDefault();
                }

                // UnitDeployment removed - using Brigade system now
                // var defenderDeployment = defenderToken != null ? await _context.UnitDeployments... : null;

                // ===== DEFENSE ELEMENTS =====
                // Get all defense elements in the area (unified model with Category field)
                var allDefenseElements = defenderToken != null
                    ? await _context.DefenseElements
                        .Where(de => de.IsActive && de.TeamId == defenderToken.TeamId && de.Status == "active")
                        .ToListAsync()
                    : new List<DefenseElement>();

                var defenseElements = new
                {
                    killZones = allDefenseElements
                        .Where(de => de.Category.ToLower() == "killzone")
                        .Select(de => new
                        {
                            id = de.Id,
                            elementId = de.ElementId,
                            type = de.Type,
                            strength = de.Strength,
                            effectiveness = de.Effectiveness,
                            coordinates = de.Coordinates,
                            metadata = de.Metadata,
                            notes = de.Notes
                        })
                        .ToList(),

                    minefields = allDefenseElements
                        .Where(de => de.Category.ToLower() == "minefield")
                        .Select(de => new
                        {
                            id = de.Id,
                            elementId = de.ElementId,
                            type = de.Type,
                            strength = de.Strength,
                            effectiveness = de.Effectiveness,
                            coordinates = de.Coordinates,
                            metadata = de.Metadata,
                            notes = de.Notes
                        })
                        .ToList(),

                    obstacles = allDefenseElements
                        .Where(de => de.Category.ToLower() == "obstacle")
                        .Select(de => new
                        {
                            id = de.Id,
                            elementId = de.ElementId,
                            type = de.Type,
                            strength = de.Strength,
                            effectiveness = de.Effectiveness,
                            coordinates = de.Coordinates,
                            metadata = de.Metadata,
                            notes = de.Notes
                        })
                        .ToList(),

                    defensivePositions = allDefenseElements
                        .Where(de => de.Category.ToLower() == "position")
                        .Select(de => new
                        {
                            id = de.Id,
                            elementId = de.ElementId,
                            type = de.Type,
                            strength = de.Strength,
                            effectiveness = de.Effectiveness,
                            coordinates = de.Coordinates,
                            metadata = de.Metadata,
                            notes = de.Notes
                        })
                        .ToList()
                };

                // ===== RETURN COMPREHENSIVE DATA =====
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        // Attack Order
                        attackOrder = new
                        {
                            id = attackOrder.Id,
                            status = attackOrder.Status,
                            intentJson = attackOrder.IntentJson,
                            firesJson = attackOrder.FiresJson,
                            movementJson = attackOrder.MovementJson,
                            timingJson = attackOrder.TimingJson,
                            fogOfWarJson = attackOrder.FogOfWarJson,
                            logisticsJson = attackOrder.LogisticsJson,
                            roeJson = attackOrder.ROEJson
                        },

                        // Attacker
                        attacker = new
                        {
                            tokenId = attackerToken?.Id,
                            name = attackerToken?.Name,
                            unitDesignation = attackerToken?.UnitDesignation,
                            unitType = attackerToken?.UnitType,
                            forceType = attackerToken?.ForceType,
                            organizationLevel = attackerToken?.OrganizationLevel,
                            location = attackerLocation != null ? new
                            {
                                latitude = attackerLocation.latitude,
                                longitude = attackerLocation.longitude,
                                position = attackerLocation.Position
                            } : null,
                            deployment = (object?)null // UnitDeployment removed - use Brigade system
                        },

                        // Defender
                        defender = new
                        {
                            tokenId = defenderToken?.Id,
                            name = defenderToken?.Name,
                            unitDesignation = defenderToken?.UnitDesignation,
                            unitType = defenderToken?.UnitType,
                            forceType = defenderToken?.ForceType,
                            organizationLevel = defenderToken?.OrganizationLevel,
                            isSuspectedToken = isSuspectedToken,
                            detectionConfidence = detectionConfidence,
                            location = defenderLocation != null ? new
                            {
                                latitude = defenderLocation.latitude,
                                longitude = defenderLocation.longitude,
                                position = defenderLocation.Position
                            } : null,
                            deployment = (object?)null // UnitDeployment removed - use Brigade system
                        },

                        // Defense Elements
                        defenseElements = defenseElements
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comprehensive attack/defense data");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Run comprehensive attack and defense simulation with suspected token resolution
        /// Returns partial view for modal display
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RunComprehensiveSimulation(string attackOrderId)
        {
            try
            {
                if (!Guid.TryParse(attackOrderId, out var orderGuid))
                {
                    return Content("<div class='alert alert-danger'>Invalid attack order ID</div>");
                }

                var attackOrder = await _context.EnhancedAttackOrders
                    .FirstOrDefaultAsync(ao => ao.Id == orderGuid);


                if (attackOrder == null)
                {
                    return Content("<div class='alert alert-danger'>Attack order not found</div>");
                }

                _logger.LogInformation("🎯 Running simulation for attack order {OrderId}", orderGuid);
                _logger.LogInformation("  Attacker Token: {AttackerId}", attackOrder.AttackerTokenId);
                _logger.LogInformation("  Defender Token: {DefenderId}", attackOrder.TargetTokenId);

                // Check brigade linkage for debugging
                var attackerBrigade = await _context.Brigades
                    .Include(b => b.InfantryBattalions)
                    .Include(b => b.ArmouredRegiments)
                    .Include(b => b.ArtilleryRegiments)
                    .FirstOrDefaultAsync(b => b.TokenId == attackOrder.AttackerTokenId);

                Guid? TargerToken = _context.SuspectedTokens.FirstOrDefault(x => x.Id == attackOrder.TargetTokenId)?.RealTokenId;

                if (TargerToken != null)
                {

                }
                var defenderBrigade = await _context.Brigades
                    .Include(b => b.InfantryBattalions)
                    .Include(b => b.ArmouredRegiments)
                    .Include(b => b.ArtilleryRegiments)
                    .FirstOrDefaultAsync(b => b.TokenId == TargerToken);

                if (attackerBrigade != null)
                {
                    _logger.LogInformation("✅ Attacker Brigade Found: {Count} infantry, {Armour} armour, {Artillery} artillery", 
                        attackerBrigade.InfantryBattalions.Count, 
                        attackerBrigade.ArmouredRegiments.Count, 
                        attackerBrigade.ArtilleryRegiments.Count);
                }
                else
                {
                    _logger.LogWarning("⚠️ NO BRIGADE FOUND for attacker token {TokenId}", attackOrder.AttackerTokenId);
                }

                if (defenderBrigade != null)
                {
                    _logger.LogInformation("✅ Defender Brigade Found: {Count} infantry, {Armour} armour, {Artillery} artillery", 
                        defenderBrigade.InfantryBattalions.Count, 
                        defenderBrigade.ArmouredRegiments.Count, 
                        defenderBrigade.ArtilleryRegiments.Count);
                }
                else
                {
                    _logger.LogWarning("⚠️ NO BRIGADE FOUND for defender token {TokenId}", attackOrder.TargetTokenId);
                }

                var result = await _simulationService.SimulateAttackDefenseAsync(attackOrder.AttackerTokenId, Guid.Parse(TargerToken.ToString()));

                if (!result.Success)
                {
                    return Content($"<div class='alert alert-danger'>{result.Message}</div>");
                }

                // Map to ViewModel
                var viewModel = new CombatSimulationResultsViewModel
                {
                    AttackerTokenName = result.AttackerTokenName,
                    DefenderTokenName = result.DefenderTokenName,
                    WasSuspectedToken = result.WasSuspectedToken,
                    DetectionConfidence = result.DetectionConfidence,
                    SimulationTime = result.SimulationTime,
                    AttackPhases = result.AttackPhases.Select(p => new AttackPhaseViewModel
                    {
                        PhaseName = p.PhaseName,
                        PhaseType = p.PhaseType,
                        Location = p.Location,
                        DelayMinutes = p.DelayMinutes,
                        CasualtiesAttacker = p.CasualtiesAttacker,
                        CasualtiesDefender = p.CasualtiesDefender,
                        Notes = p.Notes
                    }).ToList(),
                    DefensePhases = result.DefensePhases.Select(p => new DefensePhaseViewModel
                    {
                        PhaseName = p.PhaseName,
                        PhaseType = p.PhaseType,
                        TimeToStayMinutes = p.TimeToStayMinutes,
                        MovementDelayMinutes = p.MovementDelayMinutes,
                        CounterAttackDelayMinutes = p.CounterAttackDelayMinutes,
                        CasualtiesDefender = p.CasualtiesDefender,
                        CasualtiesAttacker = p.CasualtiesAttacker,
                        Notes = p.Notes
                    }).ToList(),
                    AttackSummary = new AttackSummaryViewModel
                    {
                        EngagementKillZoneSummary = result.AttackSummary.EngagementKillZoneSummary,
                        DefensePositionsSummary = result.AttackSummary.DefensePositionsSummary,
                        TotalDelayMinutes = result.AttackSummary.TotalDelayMinutes,
                        TotalAttackerCasualties = result.AttackSummary.TotalAttackerCasualties,
                        TotalDefenderCasualties = result.AttackSummary.TotalDefenderCasualties
                    },
                    DefenseSummary = new DefenseSummaryViewModel
                    {
                        TimeToStaySummary = result.DefenseSummary.TimeToStaySummary,
                        CounterPenetrationMovementSummary = result.DefenseSummary.CounterPenetrationMovementSummary,
                        CounterAttackSummary = result.DefenseSummary.CounterAttackSummary,
                        TotalTimeMinutes = result.DefenseSummary.TotalTimeMinutes,
                        TotalDefenderCasualties = result.DefenseSummary.TotalDefenderCasualties,
                        TotalAttackerCasualties = result.DefenseSummary.TotalAttackerCasualties
                    },
                    // Victory Condition Results
                    VictoryOutcome = result.VictoryOutcome,
                    VictoryReason = result.VictoryReason,
                    TotalCombatRounds = result.TotalCombatRounds,
                    TotalEngagementTimeMinutes = result.TotalEngagementTimeMinutes,
                    FinalAttackerCasualtiesPercent = result.FinalAttackerCasualtiesPercent,
                    FinalDefenderCasualtiesPercent = result.FinalDefenderCasualtiesPercent
                };

                return PartialView("Partials/_SimulationResultsModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running comprehensive simulation");
                return Content($"<div class='alert alert-danger'>Simulation failed: {ex.Message}</div>");
            }
        }
    }

    /// <summary>
    /// Request model for running simulation
    /// </summary>
    public class RunSimulationRequest
    {
        /// <summary>
        /// Optional: Use an existing EnhancedAttackOrder
        /// </summary>
        public string? AttackOrderId { get; set; }
        
        /// <summary>
        /// Manual selection: Attacker token ID
        /// </summary>
        public string? AttackerTokenId { get; set; }
        
        /// <summary>
        /// Manual selection: Target token ID (can be real token or suspected token)
        /// </summary>
        public string? TargetTokenId { get; set; }
    }

    /// <summary>
    /// Request model for saving draft data
    /// </summary>
    public class SaveDraftRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string TabName { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public Guid? AttackerTokenId { get; set; }
        public Guid? TargetTokenId { get; set; }
    }

    public class UpdateAttackOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public Guid? AttackerTokenId { get; set; }
        public Guid? TargetTokenId { get; set; }
    }

    public class DeleteAttackOrderRequest
    {
        public Guid OrderId { get; set; }
    }
}
