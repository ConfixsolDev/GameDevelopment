using Microsoft.AspNetCore.Mvc;
using TechWebSol.Models.AttackPlanning;
using TechWebSol.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// Controller for handling attack planning UI and operations
    /// </summary>
    public class AttackPlanningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttackPlanningController> _logger;

        public AttackPlanningController(ApplicationDbContext context, ILogger<AttackPlanningController> logger)
        {
            _context = context;
            _logger = logger;
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
        /// Save draft attack order (auto-save or manual save)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request)
        {
            try
            {
                _logger.LogInformation($"Saving draft for order ID: {request.OrderId}, Tab: {request.TabName}");

                // Check if order already exists
                var existingOrder = await _context.EnhancedAttackOrders
                    .FirstOrDefaultAsync(o => o.Id.ToString() == request.OrderId);

                if (existingOrder == null)
                {
                    // Create new order
                    existingOrder = new EnhancedAttackOrder
                    {
                        Id = Guid.Parse(request.OrderId),
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
                        attackType = order.Intent?.AttackType,
                        maneuverForm = order.Intent?.ManeuverForm,
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
            if (data.ContainsKey("AttackType"))
                intent.AttackType = data["AttackType"]?.ToString();
            if (data.ContainsKey("ManeuverForm"))
                intent.ManeuverForm = data["ManeuverForm"]?.ToString();
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
    }

    /// <summary>
    /// Request model for saving draft data
    /// </summary>
    public class SaveDraftRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string TabName { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
