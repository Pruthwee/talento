using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Loads configuration from AWS Systems Manager Parameter Store at application startup.
    /// This replaces Web.config transformation files by externalizing configuration
    /// to Parameter Store, enabling environment-specific configuration without rebuilds.
    /// </summary>
    public static class ParameterStoreConfig
    {
        private static bool _loaded = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Loads parameters from Parameter Store and merges them into AppSettings.
        /// Called once at application startup.
        /// </summary>
        public static void LoadParameters()
        {
            lock (_lock)
            {
                if (_loaded) return;
                _loaded = true;

                EnsureParametersLoaded();
            }
        }

        /// <summary>
        /// Ensures parameters are loaded from Parameter Store before reading a value.
        /// </summary>
        private static void EnsureParametersLoaded()
        {
            var parameterPath = ConfigurationManager.AppSettings["App:ParameterPath"];
            if (string.IsNullOrEmpty(parameterPath))
            {
                parameterPath = "/TrianzOperations/";
            }

            var region = ConfigurationManager.AppSettings["AWS:Region"] ?? "us-east-1";

            try
            {
                using (var ssmClient = new AmazonSimpleSystemsManagementClient(
                    Amazon.RegionEndpoint.GetBySystemName(region)))
                {
                    var request = new GetParametersByPathRequest
                    {
                        Path = parameterPath,
                        Recursive = true,
                        WithDecryption = true
                    };

                    var parameters = new List<Parameter>();
                    string nextToken = null;

                    do
                    {
                        request.NextToken = nextToken;
                        var response = ssmClient.GetParametersByPath(request);
                        parameters.AddRange(response.Parameters);
                        nextToken = response.NextToken;
                    } while (!string.IsNullOrEmpty(nextToken));

                    foreach (var param in parameters)
                    {
                        // Convert Parameter Store path to AppSettings key
                        // e.g., /TrianzOperations/App/Talento -> App:Talento
                        var key = param.Name
                            .Replace(parameterPath, "")
                            .TrimStart('/')
                            .Replace("/", ":");

                        // Only set if not already in AppSettings (environment variables take precedence)
                        if (ConfigurationManager.AppSettings[key] == null)
                        {
                            ConfigurationManager.AppSettings[key] = param.Value;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Parameter Store not available (e.g., local development)
                // Configuration falls back to Web.config AppSettings
            }
        }

        /// <summary>
        /// Reads an application setting from the configuration that has been
        /// externalized to AWS Systems Manager Parameter Store. This replaces
        /// direct ConfigurationManager.AppSettings lookups that relied on
        /// Web.config transformation files.
        /// </summary>
        /// <param name="key">The configuration key to look up.</param>
        /// <returns>The configuration value, or null if not found.</returns>
        public static string GetAppSetting(string key)
        {
            EnsureParametersLoaded();
            return ConfigurationManager.AppSettings[key];
        }
    }
}
