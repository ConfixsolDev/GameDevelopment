using TechWebSol.Extensions;
using TechWebSol.Helpers;
using TechWebSol.ViewModels;

namespace TechWebSol.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession Session => _httpContextAccessor.HttpContext.Session;
        public string SessionKeyName { get; } = Constants.AppConstants.UserSessionKey;

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationUserVM GetCurrentUser()
        {
            return Session.GetObject<ApplicationUserVM>(SessionKeyName);
        }

        public IEnumerable<MvcControllerInfoArea> GetCurrentRole()
        {
            var currentUser = GetCurrentUser();
            return currentUser != null ? RolesList.GetValue(currentUser.RoleName) : null;
        }

        public void CreateSession(ApplicationUserVM user)
        {
            Session.SetObject(SessionKeyName, user);
        }

        public void ClearCurrentUser()
        {
            Session.Remove(SessionKeyName);
        }
    }
}
