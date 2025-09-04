using TechWebSol.ViewModels;

namespace TechWebSol.Services
{
    public interface IMvcControllerDiscovery
    {
        IEnumerable<MvcControllerInfo> GetControllers();
    }
}
