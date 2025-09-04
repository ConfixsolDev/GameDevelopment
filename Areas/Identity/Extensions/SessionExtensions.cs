using Newtonsoft.Json;

namespace TechWebSol.Areas.Identity.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default :
                JsonConvert.DeserializeObject<T>(value);
        }

    }
}
