using TechWebSol.ViewModels;

namespace TechWebSol.Helpers
{
    public static class SessionMessage
    {
        public static void InitiateSessionMessage(PageAlertType alertType, string title, string message)
        {
            // This would typically set a session message
            // For now, we'll implement a simple version
        }
    }

    public enum PageAlertType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
