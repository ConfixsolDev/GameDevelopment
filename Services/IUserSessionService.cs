using TechWebSol.ViewModels;

namespace TechWebSol.Services
{
    public interface IUserSessionService
    {
        ApplicationUserVM GetCurrentUser();
        void SetCurrentUser(ApplicationUserVM user);
        void ClearCurrentUser();
    }
}
