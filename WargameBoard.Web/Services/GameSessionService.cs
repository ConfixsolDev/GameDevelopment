using Microsoft.EntityFrameworkCore;
using WargameBoard.Core.Data;
using WargameBoard.Core.Entities;

namespace WargameBoard.Web.Services;

public class GameSessionService
{
    private readonly WargameDbContext _db;
    public GameSessionService(WargameDbContext db) => _db = db;

    /// <summary>Create a live session and Turn #1 for a scenario.</summary>
    public async Task<Session> StartSessionAsync(int scenarioId, int? startingSideId = null, string? notes = null)
    {
        var scenario = await _db.Scenarios
            .Include(s => s.ScenarioObjectives)
            .FirstOrDefaultAsync(s => s.Id == scenarioId);
        if (scenario == null) throw new InvalidOperationException("Scenario not found.");

        // Decide who starts: provided side > side of first objective > null
        var currentSideId = startingSideId
                            ?? scenario.ScenarioObjectives.OrderBy(o => o.Id).FirstOrDefault()?.SideId;

        var session = new Session
        {
            ScenarioId = scenario.Id,
            CurrentSideId = currentSideId,
            Notes = notes ?? $"Session for {scenario.Name}",
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        var turn1 = new Turn
        {
            SessionId = session.Id,
            Number = 1,
            StartedAt = DateTime.UtcNow
        };
        _db.Turns.Add(turn1);
        await _db.SaveChangesAsync();

        // Reload with navs
        return await _db.Sessions
            .Include(s => s.Scenario)
            .Include(s => s.Turns)
            .FirstAsync(s => s.Id == session.Id);
    }

    /// <summary>End current turn and start next.</summary>
    public async Task<Turn> AdvanceTurnAsync(int sessionId)
    {
        var session = await _db.Sessions
            .Include(s => s.Turns)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new InvalidOperationException("Session not found.");

        var current = session.Turns.OrderByDescending(t => t.Number).FirstOrDefault();
        if (current != null && current.EndedAt == null)
            current.EndedAt = DateTime.UtcNow;

        var next = new Turn
        {
            SessionId = session.Id,
            Number = (current?.Number ?? 0) + 1,
            StartedAt = DateTime.UtcNow
        };
        _db.Turns.Add(next);
        await _db.SaveChangesAsync();
        return next;
    }

    /// <summary>End the session (soft-close).</summary>
    public async Task EndSessionAsync(int sessionId)
    {
        var session = await _db.Sessions.FindAsync(sessionId)
                      ?? throw new InvalidOperationException("Session not found.");
        if (session.EndedAt == null) session.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
