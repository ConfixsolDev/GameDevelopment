using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechWebSol.Data;
using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    public class TokenRepository: ITokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenIdentificationDAL _tokenDAL;

        public TokenRepository(ApplicationDbContext context, TokenIdentificationDAL tokenDAL)
        {
            _context = context;
            _tokenDAL = tokenDAL;
        }

        public async Task<IEnumerable<Token>> GetAllTokensAsync()
        {
            return await _context.Tokens
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.Stability)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchProperties)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchPattern)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.MultiTouchGeometry)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Token?> GetTokenByIdAsync(long id)
        {
            return await _context.Tokens
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.Stability)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchProperties)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchPattern)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.MultiTouchGeometry)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Token?> GetTokenByNameAsync(string name)
        {
            return await _context.Tokens
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.Stability)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchProperties)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchPattern)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.MultiTouchGeometry)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Token> CreateTokenAsync(Token token)
        {
            // Convert Token to UnifiedTokenSaveRequest and use unified DAL
            var request = new UnifiedTokenSaveRequest
            {
                Name = token.Name,
                Description = token.Description,
                Category = token.Category,
                IsActive = token.IsActive,
                TokenGroupId = token.TokenGroupId,
                // Note: TouchPoints would need to be provided separately for pattern creation
                // This method assumes the token is already created with signature
            };

            var result = await _tokenDAL.SaveTokenAsync(request);
            
            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed to create token: {result.Message}");
            }

            // Return the created token
            return await GetTokenByIdAsync(result.TokenId!.Value) 
                ?? throw new InvalidOperationException("Token was created but could not be retrieved");
        }

        public async Task<Token> UpdateTokenAsync(Token token)
        {
            // Convert Token to UnifiedTokenSaveRequest and use unified DAL
            var request = new UnifiedTokenSaveRequest
            {
                TokenId = token.Id,
                Name = token.Name,
                Description = token.Description,
                Category = token.Category,
                IsActive = token.IsActive,
                TokenGroupId = token.TokenGroupId,
                // Note: TouchPoints would need to be provided separately for pattern updates
                // This method assumes the token signature is already updated
            };

            var result = await _tokenDAL.SaveTokenAsync(request);
            
            if (!result.Success)
            {
                throw new InvalidOperationException($"Failed to update token: {result.Message}");
            }

            // Return the updated token
            return await GetTokenByIdAsync(result.TokenId!.Value) 
                ?? throw new InvalidOperationException("Token was updated but could not be retrieved");
        }

        public async Task<bool> DeleteTokenAsync(long id)
        {
            // Use unified DAL for token deletion
            var result = await _tokenDAL.DeleteTokenAsync(id);
            return result.Success;
        }

        public async Task<bool> TokenExistsAsync(long id)
        {
            return await _context.Tokens.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> TokenNameExistsAsync(string name, long? excludeId = null)
        {
            var query = _context.Tokens.Where(t => t.Name == name);
            if (excludeId.HasValue)
                query = query.Where(t => t.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Token>> SearchTokensAsync(string searchTerm)
        {
            return await _context.Tokens
                .Where(t => t.Name.Contains(searchTerm) ||
                           (t.Description != null && t.Description.Contains(searchTerm)) ||
                           (t.Category != null && t.Category.Contains(searchTerm)))
                .Include(t => t.Signature)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TokenStatistics> GetTokenStatisticsAsync()
        {
            var totalTokens = await _context.Tokens.CountAsync();
            var activeTokens = await _context.Tokens.CountAsync(t => t.IsActive);
            var inactiveTokens = totalTokens - activeTokens;
            var lastTokenCreated = await _context.Tokens.MaxAsync(t => (DateTime?)t.CreatedAt);
            var lastTokenUsed = await _context.Tokens.MaxAsync(t => (DateTime?)t.LastUsed);

            return new TokenStatistics
            {
                TotalTokens = totalTokens,
                ActiveTokens = activeTokens,
                InactiveTokens = inactiveTokens,
                LastTokenCreated = lastTokenCreated,
                LastTokenUsed = lastTokenUsed,
                AverageConfidence = 0, // Calculate based on your needs
                TotalIdentifications = 0, // Track in a separate table
                SuccessfulIdentifications = 0, // Track in a separate table
                SuccessRate = 0 // Calculate based on your needs
            };
        }

        public async Task<Token?> IdentifyTokenAsync(TokenSignature signature, double confidenceThreshold = 70.0)
        {
            // This is a simplified identification logic
            // You would implement your actual token matching algorithm here
            var tokens = await _context.Tokens
                .Include(t => t.Signature)
                .Where(t => t.IsActive && t.Signature != null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                if (token.Signature != null)
                {
                    // Implement your comparison logic here
                    // This is just a placeholder
                    var similarity = CalculateSimilarity(signature, token.Signature);
                    if (similarity >= confidenceThreshold)
                    {
                        // Update usage statistics
                        token.LastUsed = DateTime.UtcNow;
                        token.UsageCount++;
                        await _context.SaveChangesAsync();

                        return token;
                    }
                }
            }

            return null;
        }

        private double CalculateSimilarity(TokenSignature signature1, TokenSignature signature2)
        {
            // Implement your similarity calculation logic here
            // This is just a placeholder
            if (signature1.TokenHash == signature2.TokenHash)
                return 100.0;

            return 0.0;
        }
    }
}
