using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Retrieves database credentials from AWS Secrets Manager and injects them
    /// into connection strings, replacing hard-coded passwords.
    /// </summary>
    public static class SecretsManagerHelper
    {
        /// <summary>
        /// Retrieves the database secret from AWS Secrets Manager and returns
        /// the connection string with the password populated.
        /// </summary>
        public static string GetSecureConnectionString(string connectionStringName)
        {
            var originalConnStr = ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString;
            if (string.IsNullOrEmpty(originalConnStr))
            {
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
            }

            // Check if password is already set (e.g., from environment variable)
            if (originalConnStr.Contains("password=P#$$w0rd") || 
                (originalConnStr.Contains("password=") && !originalConnStr.Contains("password=;")))
            {
                // Password already present, return as-is
                return originalConnStr;
            }

            // For EntityConnection strings, we need to modify the inner connection string
            if (connectionStringName == "TrianzOperationsEntities")
            {
                return GetSecureEntityConnectionString(originalConnStr);
            }
            else if (connectionStringName == "ADOConnectionString")
            {
                return GetSecureAdoConnectionString(originalConnStr);
            }

            return originalConnStr;
        }

        private static string GetSecureEntityConnectionString(string entityConnStr)
        {
            // Parse the EntityConnection string to get the inner connection string
            var builder = new EntityConnectionStringBuilder(entityConnStr);
            var innerConnStr = builder.ConnectionString;

            // Get credentials from Secrets Manager
            var secret = GetDatabaseSecret();
            
            // Build the inner connection string with the secret password
            var sqlBuilder = new SqlConnectionStringBuilder(innerConnStr)
            {
                Password = secret.Password,
                UserID = secret.Username
            };

            builder.ConnectionString = sqlBuilder.ConnectionString;
            return builder.ConnectionString;
        }

        private static string GetSecureAdoConnectionString(string connStr)
        {
            var secret = GetDatabaseSecret();
            var builder = new SqlConnectionStringBuilder(connStr)
            {
                Password = secret.Password,
                UserID = secret.Username
            };
            return builder.ConnectionString;
        }

        private static DatabaseSecret GetDatabaseSecret()
        {
            var secretName = ConfigurationManager.AppSettings["Database:SecretName"]
                ?? ConfigurationManager.AppSettings["AWS:SecretsManager:DatabaseSecretName"]
                ?? "/TrianzOperations/DatabaseCredentials";

            var region = ConfigurationManager.AppSettings["AWS:Region"] ?? "us-east-1";

            using (var client = new AmazonSecretsManagerClient(
                RegionEndpoint.GetBySystemName(region)))
            {
                var response = client.GetSecretValue(new GetSecretValueRequest
                {
                    SecretId = secretName
                });

                var secretJson = response.SecretString;
                var secret = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSecret>(secretJson);
                return secret;
            }
        }

        public class DatabaseSecret
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public string Database { get; set; }
        }
    }
}
