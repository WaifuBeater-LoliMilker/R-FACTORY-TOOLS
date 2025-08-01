using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace R_Factory_Tools.Utilities
{
    public static class ConnectionStringProvider
    {
        public static string Default { get; }
        static ConnectionStringProvider()
        {
            var json = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json"));

            using var doc = JsonDocument.Parse(json);

            Default = doc.RootElement
                .GetProperty("ConnectionStrings")
                .GetProperty("DefaultConnection")
                .GetString()
                ?? throw new InvalidOperationException(
                     "DefaultConnection not found in appsettings.json");
        }
    }
}
