using Microsoft.Extensions.Configuration;

namespace CommonLibrary.Helpers
{
    /// <summary>
    /// Centralized helper for loading configuration with environment-aware overrides.
    /// Placed in CommonLibrary to avoid project reference cycles.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Resolve the current environment name.
        /// Defaults to "dev" when no environment variable is set.
        /// </summary>
        public static string GetEnvironment()
        {
            var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var result = aspnetEnv ?? dotnetEnv ?? "dev";
            

            Console.WriteLine($"ASPNETCORE_ENVIRONMENT={aspnetEnv}");
            Console.WriteLine($"DOTNET_ENVIRONMENT={dotnetEnv}");
            Console.WriteLine($"Final Environment={result}");
            
            return result;
        }

        /// <summary>
        /// Determine if the current environment should enable development features (like Swagger).
        /// Returns true for: Development, dev, local, interdev.
        /// </summary>
        public static bool IsDevelopmentEnvironment()
        {
            var env = GetEnvironment().ToLowerInvariant();
            return env == "development" 
                || env == "dev" 
                || env == "local" 
                || env == "interdev";
        }

        /// <summary>
        /// Load configuration in the following order:
        /// 1. Base JSON file (required)
        /// 2. Environment-specific JSON file (optional)
        /// 3. Environment variables with EGMIOT__ prefix (optional)
        /// </summary>
        /// <param name="basePath">Base path where the JSON files are located.</param>
        /// <param name="configName">Config file name without extension.</param>
        /// <returns>Built <see cref="IConfiguration"/>.</returns>
        public static IConfiguration LoadConfiguration(string basePath, string configName)
        {
            var env = GetEnvironment();

            Console.WriteLine($"[LoadConfiguration] basePath={basePath}");
            Console.WriteLine($"[LoadConfiguration] configName={configName}");
            Console.WriteLine($"[LoadConfiguration] env={env}");
#if DEBUG
            Console.WriteLine("\n--- 載入結�? ---");
#endif
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile($"{configName}.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"{configName}.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables(prefix: "EGMIOT__")
                .Build();

#if DEBUG
            foreach (var pair in config.AsEnumerable())
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
#endif
            return config;
        }
    }
}



