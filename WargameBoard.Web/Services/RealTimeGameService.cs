using Microsoft.AspNetCore.SignalR;
using WargameBoard.Core.Data;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Hubs;

namespace WargameBoard.Web.Services
{
    public interface IRealTimeGameService
    {
        Task<bool> PlaceTokenAsync(int sessionId, int tokenPieceId, int hexId);
        Task<bool> MoveTokenAsync(int sessionId, int placementId, int newHexId);
        Task<bool> RemoveTokenAsync(int sessionId, int placementId);
        Task<bool> UpdateHexTerrainAsync(int sessionId, int hexId, int terrainTypeId);
        Task<bool> AddHexFeatureAsync(int sessionId, int hexId, int featureTypeId, string featureKind, int? sideId);
        Task<bool> RemoveHexFeatureAsync(int sessionId, int featureId);
        Task<bool> AdvanceTurnAsync(int sessionId);
        Task<bool> UpdateObjectiveControlAsync(int sessionId, int objectiveId, int sideId, string action);
    }

    public class RealTimeGameService : IRealTimeGameService
    {
        private readonly WargameDbContext _context;
        private readonly IHubContext<RealTimeGameHub> _hubContext;
        private readonly ILogger<RealTimeGameService> _logger;

        public RealTimeGameService(
            WargameDbContext context, 
            IHubContext<RealTimeGameHub> hubContext,
            ILogger<RealTimeGameService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<bool> PlaceTokenAsync(int sessionId, int tokenPieceId, int hexId)
        {
            try
            {
                var placement = new Placement
                {
                    SessionId = sessionId,
                    TokenPieceId = tokenPieceId,
                    HexId = hexId,
                    PlacedAt = DateTime.UtcNow
                };

                _context.Placements.Add(placement);
                await _context.SaveChangesAsync();

                // Broadcast to all clients in the session
                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("TokenPlaced", new
                {
                    Id = placement.Id,
                    TokenPieceId = tokenPieceId,
                    HexId = hexId,
                    PlacedAt = placement.PlacedAt
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing token");
                return false;
            }
        }

        public async Task<bool> MoveTokenAsync(int sessionId, int placementId, int newHexId)
        {
            try
            {
                var placement = await _context.Placements.FindAsync(placementId);
                if (placement == null) return false;

                var moveEvent = new MoveEvent
                {
                    SessionId = sessionId,
                    TokenPieceId = placement.TokenPieceId,
                    FromHexId = placement.HexId,
                    ToHexId = newHexId,
                    Timestamp = DateTime.UtcNow
                };

                _context.MoveEvents.Add(moveEvent);
                placement.HexId = newHexId;
                _context.Placements.Update(placement);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("TokenMoved", new
                {
                    PlacementId = placementId,
                    FromHexId = moveEvent.FromHexId,
                    ToHexId = newHexId,
                    Timestamp = moveEvent.Timestamp
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving token");
                return false;
            }
        }

        public async Task<bool> RemoveTokenAsync(int sessionId, int placementId)
        {
            try
            {
                var placement = await _context.Placements.FindAsync(placementId);
                if (placement == null) return false;

                _context.Placements.Remove(placement);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("TokenRemoved", placementId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token");
                return false;
            }
        }

        public async Task<bool> UpdateHexTerrainAsync(int sessionId, int hexId, int terrainTypeId)
        {
            try
            {
                var hex = await _context.Hexes.FindAsync(hexId);
                if (hex == null) return false;

                hex.TerrainTypeId = terrainTypeId;
                _context.Hexes.Update(hex);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("HexUpdated", new
                {
                    Id = hexId,
                    Q = hex.Q,
                    R = hex.R,
                    TerrainTypeId = terrainTypeId
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hex terrain");
                return false;
            }
        }

        public async Task<bool> AddHexFeatureAsync(int sessionId, int hexId, int featureTypeId, string featureKind, int? sideId)
        {
            try
            {
                var feature = new HexFeature
                {
                    HexId = hexId,
                    FeatureKind = Enum.Parse<FeatureKind>(featureKind),
                    SideId = sideId
                };

                if (featureKind == "Fort")
                    feature.FortificationTypeId = featureTypeId;
                else if (featureKind == "Obstacle")
                    feature.ObstacleTypeId = featureTypeId;

                _context.HexFeatures.Add(feature);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("HexFeatureAdded", new
                {
                    Id = feature.Id,
                    HexId = hexId,
                    FeatureKind = featureKind,
                    TypeId = featureTypeId,
                    SideId = sideId
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding hex feature");
                return false;
            }
        }

        public async Task<bool> RemoveHexFeatureAsync(int sessionId, int featureId)
        {
            try
            {
                var feature = await _context.HexFeatures.FindAsync(featureId);
                if (feature == null) return false;

                _context.HexFeatures.Remove(feature);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("HexFeatureRemoved", featureId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing hex feature");
                return false;
            }
        }

        public async Task<bool> AdvanceTurnAsync(int sessionId)
        {
            try
            {
                var currentTurn = await _context.Turns
                    .Where(t => t.SessionId == sessionId)
                    .OrderByDescending(t => t.Number)
                    .FirstOrDefaultAsync();

                if (currentTurn != null && currentTurn.EndedAt == null)
                {
                    currentTurn.EndedAt = DateTime.UtcNow;
                    _context.Turns.Update(currentTurn);
                }

                var newTurn = new Turn
                {
                    SessionId = sessionId,
                    Number = (currentTurn?.Number ?? 0) + 1,
                    StartedAt = DateTime.UtcNow
                };

                _context.Turns.Add(newTurn);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("TurnAdvanced", new
                {
                    Number = newTurn.Number,
                    StartedAt = newTurn.StartedAt,
                    EndedAt = newTurn.EndedAt
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing turn");
                return false;
            }
        }

        public async Task<bool> UpdateObjectiveControlAsync(int sessionId, int objectiveId, int sideId, string action)
        {
            try
            {
                var log = new ObjectiveControlLog
                {
                    SessionId = sessionId,
                    ObjectiveId = objectiveId,
                    SideId = sideId,
                    GainedLost = action == "gained" ? GainLoss.Gained : GainLoss.Lost,
                    Timestamp = DateTime.UtcNow
                };

                _context.ObjectiveControlLogs.Add(log);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("ObjectiveControlUpdated", new
                {
                    ObjectiveId = objectiveId,
                    SideId = sideId,
                    Action = action,
                    Timestamp = log.Timestamp
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating objective control");
                return false;
            }
        }
    }
}
