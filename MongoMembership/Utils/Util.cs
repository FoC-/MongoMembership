using System;
using System.Configuration;
using System.Linq;

namespace MongoMembership.Utils
{
    internal static class Util
    {
        public static T ConvertOrDefault<T>(this string value, T defaultValue)
        {
            return value.IsNullOrEmpty()
                ? defaultValue
                : (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetConnectionStringByName(string connectionsettingskeys)
        {
            var connectionString = connectionsettingskeys
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => k.IsNotNullOrEmpty())
                .SelectMany(k => new[]
                    {
                        ConfigurationManager.ConnectionStrings[k]?.ConnectionString,
                        ConfigurationManager.AppSettings[k]
                    })
                .FirstOrDefault(v => v.IsNotNullOrEmpty());
            return connectionString ?? "mongodb://localhost/MongoMembership";
        }
    }
}
