using System.Collections.Concurrent;
using TechWebSol.ViewModels;

namespace TechWebSol.Helpers
{
    public static class RolesList
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<MvcControllerInfoArea>> Roles
            = new ConcurrentDictionary<string, IEnumerable<MvcControllerInfoArea>>();

        public static void AddValue(string key, IEnumerable<MvcControllerInfoArea> values)
        {
            // Using AddOrUpdate to handle concurrent adds/updates safely.
            Roles.AddOrUpdate(key, values, (existingKey, existingVal) => values);
        }

        public static IEnumerable<MvcControllerInfoArea> GetValue(string key)
        {
            // Direct return with conditional access to avoid returning null.
            return Roles.TryGetValue(key, out var values) ? values : Enumerable.Empty<MvcControllerInfoArea>();
        }
    }
}
