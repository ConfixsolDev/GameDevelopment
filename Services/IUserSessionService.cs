using TechWebSol.ViewModels;

namespace TechWebSol.Services
{
    public interface IUserSessionService
    {
        ApplicationUserVM GetCurrentUser();
        IEnumerable<MvcControllerInfoArea> GetCurrentRole();
        void CreateSession(ApplicationUserVM user);
        void ClearCurrentUser();
    }
}
