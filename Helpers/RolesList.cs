using TechWebSol.ViewModels;

namespace TechWebSol.Helpers
{
    public static class RolesList
    {
        private static readonly Dictionary<string, object> _roles = new Dictionary<string, object>();

        public static void AddValue(string roleName, object permissions)
        {
            if (!_roles.ContainsKey(roleName))
            {
                _roles[roleName] = permissions;
            }
        }

        public static object GetValue(string roleName)
        {
            return _roles.TryGetValue(roleName, out var value) ? value : null;
        }

        public static void Clear()
        {
            _roles.Clear();
        }
    }
}
