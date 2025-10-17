using TechWebSol.Models;

namespace TechWebSol.Services.TokenManagement
{
    public interface ITokenRepository
    {
        Task<IEnumerable<Token>> GetAllTokensAsync();
        Task<Token?> GetTokenByIdAsync(Guid id);
        Task<Token?> GetTokenByNameAsync(string name);
        Task<Token> CreateTokenAsync(Token token);
        Task<Token> UpdateTokenAsync(Token token);
        Task<bool> DeleteTokenAsync(Guid id);
        Task<bool> TokenExistsAsync(Guid id);
        Task<bool> TokenNameExistsAsync(string name, Guid? excludeId = null);
        Task<IEnumerable<Token>> SearchTokensAsync(string searchTerm);
        Task<TokenStatistics> GetTokenStatisticsAsync();
        Task<Token?> IdentifyTokenAsync(TokenSignature signature, double confidenceThreshold = 70.0);
    }
}
