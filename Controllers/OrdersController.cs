using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;
using System.Text.Json;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly IAttackPreviewService _attackPreviewService;
        private readonly IOrderPersistenceService _orderPersistenceService;
        private readonly IDetectionService _detectionService;
        private readonly ILogger<OrdersController> _logger;
        private readonly ApplicationUserVM user;

        public OrdersController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            IAttackPreviewService attackPreviewService,
            IOrderPersistenceService orderPersistenceService,
            IDetectionService detectionService,
            ILogger<OrdersController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _attackPreviewService = attackPreviewService;
            _orderPersistenceService = orderPersistenceService;
            _detectionService = detectionService;
            _logger = logger;
            user = userSessionService.GetCurrentUser();
        }

        /// <summary>
        /// Preview attack outcome without making any changes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PreviewAttackToken(
            [FromQuery] string attackerId,
            [FromQuery] string targetId,
            [FromQuery] int startTurn = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(attackerId) || string.IsNullOrEmpty(targetId))
                {
                    return BadRequest(new { success = false, message = "AttackerId and TargetId are required" });
                }

                // Parse string IDs to Guid
                if (!Guid.TryParse(attackerId, out var attackerGuid) || !Guid.TryParse(targetId, out var targetGuid))
                {
                    return BadRequest(new { success = false, message = "Invalid token ID format" });
                }

                // Validate tokens exist and user has access
                var attackerToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Id == attackerGuid);
                var targetToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Id == targetGuid);

                if (attackerToken == null || targetToken == null)
                {
                    return NotFound(new { success = false, message = "Attacker or target token not found" });
                }

                // Check if user can target (fog of war check)
                var canTarget = await _detectionService.CanTargetAsync(attackerId, targetId, 0.5);
                
                // Get preview result
                var preview = await _attackPreviewService.PreviewTokenAttackAsync(attackerId, targetId, startTurn, null);

                // If hard policy and cannot target, return 403
                if (!canTarget)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Target cannot be reliably detected",
                        preview = preview
                    });
                }

                return Json(new
                {
                    success = true,
                    preview = preview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing attack from {AttackerId} to {TargetId}", attackerId, targetId);
                return StatusCode(500, new { success = false, message = "Error calculating attack preview" });
            }
        }

        /// <summary>
        /// Plan an attack order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlanAttackToken([FromBody] PlanAttackRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                // Parse string IDs to Guid
                if (!Guid.TryParse(request.AttackerId, out var attackerGuid) || !Guid.TryParse(request.TargetId, out var targetGuid))
                {
                    return BadRequest(new { success = false, message = "Invalid token ID format" });
                }

                // Validate attacker and target tokens exist
                var attackerToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Id == attackerGuid);
                var targetToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Id == targetGuid);

                if (attackerToken == null || targetToken == null)
                {
                    return NotFound(new { success = false, message = "Attacker or target token not found" });
                }

                // Check detection confidence
                var detectionConfidence = await _detectionService.GetDetectionConfidenceAsync(request.AttackerId, request.TargetId);
                var canTarget = await _detectionService.CanTargetAsync(request.AttackerId, request.TargetId, 0.5);

                // Get preview for validation
                var preview = await _attackPreviewService.PreviewTokenAttackAsync(
                    request.AttackerId, request.TargetId, request.ExpectedStartTurn, request.ArtilleryAttached?.ToArray());

                // Create attack order
                var attackOrder = new AttackOrder
                {
                    AttackerTokenId = attackerGuid,
                    TargetTokenId = targetGuid,
                    AxisId = request.AxisId,
                    ArtilleryAttached = JsonSerializer.Serialize(request.ArtilleryAttached),
                    MpReservePercent = (decimal)request.MpReservePercent,
                    Posture = request.Posture,
                    ExpectedStartTurn = request.ExpectedStartTurn,
                    DurationTurns = request.DurationTurns,
                    ExecutionMode = request.ExecutionMode,
                    Status = request.ExecutionMode == "ExecuteNow" ? "Executing" : "Planned",
                    DetectionConfidence = (decimal)detectionConfidence,
                    IsLowConfidence = !canTarget,
                    TeamId = user.TeamId,
                    CreatedBy = user.FullName,
                    PayloadJson = JsonSerializer.Serialize(new { Notes = request.Notes })
                };

                // Save the order
                var orderId = await _orderPersistenceService.SaveAttackOrderAsync(attackOrder);

                // If ExecuteNow, trigger immediate execution
                if (request.ExecutionMode == "ExecuteNow")
                {
                    // This would call the SimulationController to execute the attack
                    // For now, we'll just mark it as completed
                    await _orderPersistenceService.UpdateStatusAsync(orderId, "Completed");
                }

                _logger.LogInformation("Attack order planned: {OrderId} from {AttackerId} to {TargetId}", 
                    orderId, request.AttackerId, request.TargetId);

                return Json(new PlanAttackResponse
                {
                    OrderId = orderId,
                    Status = attackOrder.Status,
                    Message = "Attack order planned successfully",
                    Preview = preview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error planning attack from {AttackerId} to {TargetId}", 
                    request.AttackerId, request.TargetId);
                return StatusCode(500, new { success = false, message = "Error planning attack" });
            }
        }

        /// <summary>
        /// Get attack orders for a specific token
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttackOrders([FromQuery] string tokenId, [FromQuery] string type = "attacker")
        {
            try
            {
                IEnumerable<AttackOrder> orders;

                if (type == "attacker")
                {
                    orders = await _orderPersistenceService.GetOrdersByAttackerAsync(tokenId);
                }
                else
                {
                    orders = await _orderPersistenceService.GetOrdersByTargetAsync(tokenId);
                }

                return Json(new
                {
                    success = true,
                    orders = orders.Select(o => new
                    {
                        o.Id,
                        o.AttackerTokenId,
                        o.TargetTokenId,
                        o.Status,
                        o.ExpectedStartTurn,
                        o.DurationTurns,
                        o.Posture,
                        o.DetectionConfidence,
                        o.IsLowConfidence,
                        o.CreatedDate,
                        o.ExecutedUtc,
                        AttackerTokenName = o.AttackerToken?.Name,
                        TargetTokenName = o.TargetToken?.Name
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attack orders for token {TokenId}", tokenId);
                return StatusCode(500, new { success = false, message = "Error retrieving attack orders" });
            }
        }

        /// <summary>
        /// Cancel an attack order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CancelAttackOrder([FromBody] CancelOrderRequest request)
        {
            try
            {
                var success = await _orderPersistenceService.CancelOrderAsync(request.OrderId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Attack order cancelled" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to cancel attack order" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling attack order {OrderId}", request.OrderId);
                return StatusCode(500, new { success = false, message = "Error cancelling attack order" });
            }
        }

        /// <summary>
        /// Get attack order details
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttackOrder(Guid orderId)
        {
            try
            {
                var order = await _orderPersistenceService.GetAttackOrderAsync(orderId);
                
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Attack order not found" });
                }

                return Json(new
                {
                    success = true,
                    order = new
                    {
                        order.Id,
                        order.AttackerTokenId,
                        order.TargetTokenId,
                        order.AxisId,
                        ArtilleryAttached = string.IsNullOrEmpty(order.ArtilleryAttached) ? 
                            new List<string>() : JsonSerializer.Deserialize<List<string>>(order.ArtilleryAttached),
                        order.MpReservePercent,
                        order.Posture,
                        order.ExpectedStartTurn,
                        order.DurationTurns,
                        order.ExecutionMode,
                        order.Status,
                        order.DetectionConfidence,
                        order.IsLowConfidence,
                        order.CreatedDate,
                        order.ExecutedUtc,
                        AttackerTokenName = order.AttackerToken?.Name,
                        TargetTokenName = order.TargetToken?.Name,
                        Payload = string.IsNullOrEmpty(order.PayloadJson) ? 
                            null : JsonSerializer.Deserialize<object>(order.PayloadJson)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attack order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Error retrieving attack order" });
            }
        }
    }

    /// <summary>
    /// Request model for cancelling an order
    /// </summary>
    public class CancelOrderRequest
    {
        public Guid OrderId { get; set; }
    }
}
