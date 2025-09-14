using Microsoft.AspNetCore.SignalR;
using WargameBoard.Core.Data;
using WargameBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace WargameBoard.Web.Hubs
{
    public class RealTimeGameHub : Hub
    {
        private readonly WargameDbContext _context;
        private readonly ILogger<RealTimeGameHub> _logger;

        public RealTimeGameHub(WargameDbContext context, ILogger<RealTimeGameHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Join a game session
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            _logger.LogInformation($"User {Context.ConnectionId} joined session {sessionId}");
            
            // Send current session state to the new user
            var session = await GetSessionState(int.Parse(sessionId));
            await Clients.Caller.SendAsync("SessionState", session);
        }

        // Leave a game session
        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            _logger.LogInformation($"User {Context.ConnectionId} left session {sessionId}");
        }

        // Place a token on the map
        public async Task PlaceToken(string sessionId, int tokenPieceId, int hexId)
        {
            try
            {
                var session = int.Parse(sessionId);
                
                // Validate placement
                var canPlace = await ValidateTokenPlacement(session, tokenPieceId, hexId);
                if (!canPlace)
                {
                    await Clients.Caller.SendAsync("PlacementError", "Invalid token placement");
                    return;
                }

                // Create placement
                var placement = new Placement
                {
                    SessionId = session,
                    TokenPieceId = tokenPieceId,
                    HexId = hexId,
                    PlacedAt = DateTime.UtcNow,
                    PlacedByUserId = GetCurrentUserId()
                };

                _context.Placements.Add(placement);
                await _context.SaveChangesAsync();

                // Broadcast to all users in session
                var placementData = new
                {
                    Id = placement.Id,
                    TokenPieceId = tokenPieceId,
                    HexId = hexId,
                    PlacedAt = placement.PlacedAt,
                    PlacedBy = GetCurrentUserId()
                };

                await Clients.Group($"session_{sessionId}").SendAsync("TokenPlaced", placementData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing token");
                await Clients.Caller.SendAsync("PlacementError", "Failed to place token");
            }
        }

        // Move a token
        public async Task MoveToken(string sessionId, int placementId, int newHexId)
        {
            try
            {
                var session = int.Parse(sessionId);
                var placement = await _context.Placements.FindAsync(placementId);
                
                if (placement == null)
                {
                    await Clients.Caller.SendAsync("MoveError", "Placement not found");
                    return;
                }

                // Validate move
                var canMove = await ValidateTokenMove(session, placementId, newHexId);
                if (!canMove)
                {
                    await Clients.Caller.SendAsync("MoveError", "Invalid move");
                    return;
                }

                // Record move event
                var moveEvent = new MoveEvent
                {
                    SessionId = session,
                    TokenPieceId = placement.TokenPieceId,
                    FromHexId = placement.HexId,
                    ToHexId = newHexId,
                    Timestamp = DateTime.UtcNow
                };

                _context.MoveEvents.Add(moveEvent);

                // Update placement
                placement.HexId = newHexId;
                _context.Placements.Update(placement);

                await _context.SaveChangesAsync();

                // Broadcast move
                var moveData = new
                {
                    PlacementId = placementId,
                    FromHexId = moveEvent.FromHexId,
                    ToHexId = newHexId,
                    Timestamp = moveEvent.Timestamp
                };

                await Clients.Group($"session_{sessionId}").SendAsync("TokenMoved", moveData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving token");
                await Clients.Caller.SendAsync("MoveError", "Failed to move token");
            }
        }

        // Remove a token
        public async Task RemoveToken(string sessionId, int placementId)
        {
            try
            {
                var placement = await _context.Placements.FindAsync(placementId);
                if (placement == null)
                {
                    await Clients.Caller.SendAsync("RemoveError", "Placement not found");
                    return;
                }

                _context.Placements.Remove(placement);
                await _context.SaveChangesAsync();

                await Clients.Group($"session_{sessionId}").SendAsync("TokenRemoved", placementId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token");
                await Clients.Caller.SendAsync("RemoveError", "Failed to remove token");
            }
        }

        // Update hex terrain
        public async Task UpdateHexTerrain(string sessionId, int hexId, int terrainTypeId)
        {
            try
            {
                var hex = await _context.Hexes.FindAsync(hexId);
                if (hex == null)
                {
                    await Clients.Caller.SendAsync("UpdateError", "Hex not found");
                    return;
                }

                hex.TerrainTypeId = terrainTypeId;
                _context.Hexes.Update(hex);
                await _context.SaveChangesAsync();

                var hexData = new
                {
                    Id = hexId,
                    Q = hex.Q,
                    R = hex.R,
                    TerrainTypeId = terrainTypeId
                };

                await Clients.Group($"session_{sessionId}").SendAsync("HexUpdated", hexData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hex terrain");
                await Clients.Caller.SendAsync("UpdateError", "Failed to update hex");
            }
        }

        // Add hex feature
        public async Task AddHexFeature(string sessionId, int hexId, int featureTypeId, string featureKind, int? sideId)
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

                var featureData = new
                {
                    Id = feature.Id,
                    HexId = hexId,
                    FeatureKind = featureKind,
                    TypeId = featureTypeId,
                    SideId = sideId
                };

                await Clients.Group($"session_{sessionId}").SendAsync("HexFeatureAdded", featureData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding hex feature");
                await Clients.Caller.SendAsync("UpdateError", "Failed to add feature");
            }
        }

        // Remove hex feature
        public async Task RemoveHexFeature(string sessionId, int featureId)
        {
            try
            {
                var feature = await _context.HexFeatures.FindAsync(featureId);
                if (feature == null)
                {
                    await Clients.Caller.SendAsync("RemoveError", "Feature not found");
                    return;
                }

                _context.HexFeatures.Remove(feature);
                await _context.SaveChangesAsync();

                await Clients.Group($"session_{sessionId}").SendAsync("HexFeatureRemoved", featureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing hex feature");
                await Clients.Caller.SendAsync("RemoveError", "Failed to remove feature");
            }
        }

        // Advance turn
        public async Task AdvanceTurn(string sessionId)
        {
            try
            {
                var session = int.Parse(sessionId);
                var currentTurn = await _context.Turns
                    .Where(t => t.SessionId == session)
                    .OrderByDescending(t => t.Number)
                    .FirstOrDefaultAsync();

                if (currentTurn != null && currentTurn.EndedAt == null)
                {
                    currentTurn.EndedAt = DateTime.UtcNow;
                    _context.Turns.Update(currentTurn);
                }

                var newTurn = new Turn
                {
                    SessionId = session,
                    Number = (currentTurn?.Number ?? 0) + 1,
                    StartedAt = DateTime.UtcNow
                };

                _context.Turns.Add(newTurn);
                await _context.SaveChangesAsync();

                var turnData = new
                {
                    Number = newTurn.Number,
                    StartedAt = newTurn.StartedAt,
                    EndedAt = newTurn.EndedAt
                };

                await Clients.Group($"session_{sessionId}").SendAsync("TurnAdvanced", turnData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing turn");
                await Clients.Caller.SendAsync("TurnError", "Failed to advance turn");
            }
        }

        // Update objective control
        public async Task UpdateObjectiveControl(string sessionId, int objectiveId, int sideId, string action)
        {
            try
            {
                var objective = await _context.ScenarioObjectives.FindAsync(objectiveId);
                if (objective == null)
                {
                    await Clients.Caller.SendAsync("UpdateError", "Objective not found");
                    return;
                }

                var log = new ObjectiveControlLog
                {
                    SessionId = int.Parse(sessionId),
                    ObjectiveId = objectiveId,
                    SideId = sideId,
                    GainedLost = action == "gained" ? GainLoss.Gained : GainLoss.Lost,
                    Timestamp = DateTime.UtcNow
                };

                _context.ObjectiveControlLogs.Add(log);
                await _context.SaveChangesAsync();

                var objectiveData = new
                {
                    ObjectiveId = objectiveId,
                    SideId = sideId,
                    Action = action,
                    Timestamp = log.Timestamp
                };

                await Clients.Group($"session_{sessionId}").SendAsync("ObjectiveControlUpdated", objectiveData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating objective control");
                await Clients.Caller.SendAsync("UpdateError", "Failed to update objective");
            }
        }

        // Private helper methods
        private async Task<object> GetSessionState(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Scenario)
                .Include(s => s.Turns)
                .Include(s => s.CurrentSide)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.TokenPiece)
                .Include(s => s.Placements)
                    .ThenInclude(p => p.Hex)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            return new
            {
                Id = session.Id,
                Scenario = session.Scenario?.Name,
                CurrentSide = session.CurrentSide?.Name,
                Turns = session.Turns.Select(t => new
                {
                    Number = t.Number,
                    StartedAt = t.StartedAt,
                    EndedAt = t.EndedAt
                }),
                Placements = session.Placements.Select(p => new
                {
                    Id = p.Id,
                    TokenPieceId = p.TokenPieceId,
                    HexId = p.HexId,
                    Q = p.Hex?.Q,
                    R = p.Hex?.R,
                    PlacedAt = p.PlacedAt
                })
            };
        }

        private async Task<bool> ValidateTokenPlacement(int sessionId, int tokenPieceId, int hexId)
        {
            // Add your validation logic here
            // Check if token exists, if hex is valid, if placement is allowed, etc.
            return true;
        }

        private async Task<bool> ValidateTokenMove(int sessionId, int placementId, int newHexId)
        {
            // Add your movement validation logic here
            // Check movement rules, terrain costs, etc.
            return true;
        }

        private int? GetCurrentUserId()
        {
            // Implement user ID retrieval from context
            // This would typically come from authentication
            return 1; // Placeholder
        }
    }
}
