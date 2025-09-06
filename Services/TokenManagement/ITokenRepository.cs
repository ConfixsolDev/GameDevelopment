using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    public interface ITokenRepository
    {
        Task<IEnumerable<Token>> GetAllTokensAsync();
        Task<Token?> GetTokenByIdAsync(long id);
        Task<Token?> GetTokenByNameAsync(string name);
        Task<Token> CreateTokenAsync(Token token);
        Task<Token> UpdateTokenAsync(Token token);
        Task<bool> DeleteTokenAsync(long id);
        Task<bool> TokenExistsAsync(long id);
        Task<bool> TokenNameExistsAsync(string name, long? excludeId = null);
        Task<IEnumerable<Token>> SearchTokensAsync(string searchTerm);
        Task<TokenStatistics> GetTokenStatisticsAsync();
        Task<Token?> IdentifyTokenAsync(TokenSignature signature, double confidenceThreshold = 70.0);
    }
}
