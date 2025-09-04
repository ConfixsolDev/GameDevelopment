using Microsoft.AspNetCore.Http;
using TechWebSol.ViewModels;
using TechWebSol.Constants;
using TechWebSol.Extensions;

namespace TechWebSol.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationUserVM GetCurrentUser()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            return session.GetObject<ApplicationUserVM>(AppConstants.UserSessionKey);
        }

        public void SetCurrentUser(ApplicationUserVM user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetObject(AppConstants.UserSessionKey, user);
            }
        }

        public void ClearCurrentUser()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(AppConstants.UserSessionKey);
            }
        }
    }
}
