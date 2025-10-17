using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.DAL
{
    /// <summary>
    /// Data Access Layer for Defense Elements
    /// Provides unified operations for defense element management
    /// </summary>
    public class DefenseElementDAL
    {
        private readonly ApplicationDbContext _context;

        public DefenseElementDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all defense elements for a game session
        /// </summary>
        public async Task<List<DefenseElement>> GetDefenseElementsBySessionAsync(Guid gameSessionId)
        {
            return await _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .Where(d => d.GameSessionId == gameSessionId && d.Status == "active")
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get defense elements visible to a specific team based on ForceType
        /// </summary>
        public async Task<List<DefenseElement>> GetVisibleDefenseElementsAsync(Guid gameSessionId, Guid teamId, string forceType)
        {
            var query = _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .Where(d => d.GameSessionId == gameSessionId && d.Status == "active");

            // Apply visibility rules based on ForceType
            if (forceType == "Control")
            {
                // Control team can see everything (Blueland and Foxland)
                return await query.ToListAsync();
            }
            else
            {
                // Blueland and Foxland see only their own team's elements
                query = query.Where(d => d.TeamId == teamId);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get all defense elements for a team (no session filter - for when gameSessionId is null)
        /// </summary>
        public async Task<List<DefenseElement>> GetTeamDefenseElementsAsync(Guid teamId, string forceType)
        {
            var query = _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .Where(d => d.TeamId == teamId && d.Status == "active");

            // No session filter - get all active defense elements for this team
            return await query
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get defense elements associated with a specific token
        /// </summary>
        public async Task<List<DefenseElement>> GetDefenseElementsByTokenAsync(Guid tokenId)
        {
            return await _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .Where(d => d.TokenId == tokenId && d.Status == "active")
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get defense element by ID
        /// </summary>
        public async Task<DefenseElement?> GetDefenseElementByIdAsync(Guid id)
        {
            return await _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <summary>
        /// Get defense element by client-side ElementId
        /// </summary>
        public async Task<DefenseElement?> GetDefenseElementByElementIdAsync(Guid elementId, Guid gameSessionId)
        {
            return await _context.DefenseElements
                .Include(d => d.Token)
                .Include(d => d.Team)
                .FirstOrDefaultAsync(d => d.ElementId == elementId && d.GameSessionId == gameSessionId);
        }

        /// <summary>
        /// Create new defense element
        /// </summary>
        public async Task<DefenseElement> CreateDefenseElementAsync(DefenseElement defenseElement)
        {
            defenseElement.CreatedDate = DateTime.UtcNow;
            defenseElement.Status = "active";

            _context.DefenseElements.Add(defenseElement);
            await _context.SaveChangesAsync();

            return defenseElement;
        }

        /// <summary>
        /// Update existing defense element
        /// </summary>
        public async Task<DefenseElement> UpdateDefenseElementAsync(DefenseElement defenseElement)
        {
            defenseElement.UpdatedDate = DateTime.UtcNow;

            _context.DefenseElements.Update(defenseElement);
            await _context.SaveChangesAsync();

            return defenseElement;
        }

        /// <summary>
        /// Associate defense element with token (assign responsibility)
        /// </summary>
        public async Task<bool> AssociateWithTokenAsync(Guid elementId, Guid tokenId)
        {
            var element = await _context.DefenseElements.FirstOrDefaultAsync(d => d.ElementId == elementId);
            if (element == null)
                return false;

            element.TokenId = tokenId;
            element.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Dissociate defense element from token
        /// </summary>
        public async Task<bool> DissociateFromTokenAsync(Guid elementId)
        {
            var element = await _context.DefenseElements.FirstOrDefaultAsync(d => d.ElementId == elementId);
            if (element == null)
                return false;

            element.TokenId = null;
            element.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete defense element (soft delete by setting status to inactive)
        /// </summary>
        public async Task<bool> DeleteDefenseElementAsync(Guid id)
        {
            var element = await _context.DefenseElements.FindAsync(id);
            if (element == null)
                return false;

            element.Status = "inactive";
            element.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete defense element by ElementId
        /// </summary>
        public async Task<bool> DeleteDefenseElementByElementIdAsync(Guid elementId, Guid gameSessionId)
        {
            var element = await _context.DefenseElements
                .FirstOrDefaultAsync(d => d.ElementId == elementId && d.GameSessionId == gameSessionId);

            if (element == null)
                return false;

            element.Status = "inactive";
            element.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate total defense strength for a token
        /// </summary>
        public async Task<int> CalculateTokenDefenseStrengthAsync(Guid tokenId)
        {
            var elements = await _context.DefenseElements
                .Where(d => d.TokenId == tokenId && d.Status == "active")
                .ToListAsync();

            int totalStrength = 0;

            foreach (var element in elements)
            {
                // Calculate contribution based on element type
                int contribution = element.Category switch
                {
                    "killzone" => (int)(element.Strength * 0.3),
                    "minefield" => (int)(element.Strength * 0.4),
                    "obstacle" => (int)(element.Strength * 0.2),
                    "position" => (int)(element.Strength * 0.5),
                    "line" => (int)(element.Strength * 0.3),
                    _ => (int)(element.Strength * 0.2)
                };

                totalStrength += contribution;
            }

            return totalStrength;
        }

        /// <summary>
        /// Get defense elements by category
        /// </summary>
        public async Task<List<DefenseElement>> GetDefenseElementsByCategoryAsync(Guid gameSessionId, string category)
        {
            return await _context.DefenseElements
                .Include(d => d.Token)
                .Where(d => d.GameSessionId == gameSessionId && d.Category == category && d.Status == "active")
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Bulk create defense elements
        /// </summary>
        public async Task<List<DefenseElement>> CreateDefenseElementsBulkAsync(List<DefenseElement> defenseElements)
        {
            foreach (var element in defenseElements)
            {
                element.CreatedDate = DateTime.UtcNow;
                element.Status = "active";
            }

            _context.DefenseElements.AddRange(defenseElements);
            await _context.SaveChangesAsync();

            return defenseElements;
        }

        /// <summary>
        /// Clear all defense elements for a game session
        /// </summary>
        public async Task<int> ClearSessionDefenseElementsAsync(Guid gameSessionId)
        {
            var elements = await _context.DefenseElements
                .Where(d => d.GameSessionId == gameSessionId)
                .ToListAsync();

            foreach (var element in elements)
            {
                element.Status = "inactive";
                element.UpdatedDate = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync();
        }
    }
}

