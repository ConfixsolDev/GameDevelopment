using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services
{
    public interface IOrderPersistenceService
    {
        Task<Guid> SaveAttackOrderAsync(AttackOrder order);
        Task<AttackOrder?> GetAttackOrderAsync(Guid id);
        Task<IEnumerable<AttackOrder>> GetDueOrdersAsync(int currentTurn);
        Task UpdateStatusAsync(Guid id, string status);
        Task<IEnumerable<AttackOrder>> GetOrdersByAttackerAsync(string attackerTokenId);
        Task<IEnumerable<AttackOrder>> GetOrdersByTargetAsync(string targetTokenId);
        Task<bool> CancelOrderAsync(Guid orderId);
    }

    public class OrderPersistenceService : IOrderPersistenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderPersistenceService> _logger;

        public OrderPersistenceService(ApplicationDbContext context, ILogger<OrderPersistenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Save an attack order to the database
        /// </summary>
        public async Task<Guid> SaveAttackOrderAsync(AttackOrder order)
        {
            try
            {
                if (order.Id == Guid.Empty)
                {
                    order.Id = Guid.NewGuid();
                }

                order.CreatedDate = DateTime.UtcNow;
                order.IsActive = true;

                _context.AttackOrders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Attack order saved with ID {OrderId}", order.Id);
                return order.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving attack order");
                throw;
            }
        }

        /// <summary>
        /// Get an attack order by ID
        /// </summary>
        public async Task<AttackOrder?> GetAttackOrderAsync(Guid id)
        {
            try
            {
                return await _context.AttackOrders
                    .Include(ao => ao.AttackerToken)
                    .Include(ao => ao.TargetToken)
                    .Include(ao => ao.Team)
                    .FirstOrDefaultAsync(ao => ao.Id == id && ao.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attack order {OrderId}", id);
                return null;
            }
        }

        /// <summary>
        /// Get all attack orders that are due for execution at the current turn
        /// </summary>
        public async Task<IEnumerable<AttackOrder>> GetDueOrdersAsync(int currentTurn)
        {
            try
            {
                return await _context.AttackOrders
                    .Include(ao => ao.AttackerToken)
                    .Include(ao => ao.TargetToken)
                    .Where(ao => ao.IsActive && 
                                ao.Status == "Planned" && 
                                ao.ExpectedStartTurn <= currentTurn)
                    .OrderBy(ao => ao.ExpectedStartTurn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due orders for turn {CurrentTurn}", currentTurn);
                return new List<AttackOrder>();
            }
        }

        /// <summary>
        /// Update the status of an attack order
        /// </summary>
        public async Task UpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var order = await _context.AttackOrders
                    .FirstOrDefaultAsync(ao => ao.Id == id && ao.IsActive);

                if (order != null)
                {
                    order.Status = status;
                    order.LastUpdated = DateTime.UtcNow;

                    if (status == "Completed" || status == "Executed")
                    {
                        order.ExecutedUtc = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Attack order {OrderId} status updated to {Status}", id, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attack order {OrderId} status to {Status}", id, status);
                throw;
            }
        }

        /// <summary>
        /// Get all attack orders for a specific attacker token
        /// </summary>
        public async Task<IEnumerable<AttackOrder>> GetOrdersByAttackerAsync(string attackerTokenId)
        {
            try
            {
                return await _context.AttackOrders
                    .Include(ao => ao.AttackerToken)
                    .Include(ao => ao.TargetToken)
                    .Where(ao => ao.IsActive && ao.AttackerTokenId.ToString() == attackerTokenId)
                    .OrderByDescending(ao => ao.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for attacker {AttackerId}", attackerTokenId);
                return new List<AttackOrder>();
            }
        }

        /// <summary>
        /// Get all attack orders targeting a specific token
        /// </summary>
        public async Task<IEnumerable<AttackOrder>> GetOrdersByTargetAsync(string targetTokenId)
        {
            try
            {
                return await _context.AttackOrders
                    .Include(ao => ao.AttackerToken)
                    .Include(ao => ao.TargetToken)
                    .Where(ao => ao.IsActive && ao.TargetTokenId.ToString() == targetTokenId)
                    .OrderByDescending(ao => ao.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for target {TargetId}", targetTokenId);
                return new List<AttackOrder>();
            }
        }

        /// <summary>
        /// Cancel an attack order
        /// </summary>
        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _context.AttackOrders
                    .FirstOrDefaultAsync(ao => ao.Id == orderId && ao.IsActive);

                if (order == null)
                {
                    _logger.LogWarning("Attack order {OrderId} not found for cancellation", orderId);
                    return false;
                }

                if (order.Status == "Completed" || order.Status == "Executed")
                {
                    _logger.LogWarning("Cannot cancel completed attack order {OrderId}", orderId);
                    return false;
                }

                order.Status = "Cancelled";
                order.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Attack order {OrderId} cancelled", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling attack order {OrderId}", orderId);
                return false;
            }
        }
    }
}
