using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Helper class that replaces System.IO.File operations with Amazon S3 operations.
    /// Provides the same method signatures so calling code changes are minimal.
    /// </summary>
    public static class CloudFileHelper
    {
        private static readonly IAmazonS3 _s3Client;
        private static readonly string _bucketName;
        private static readonly string _keyPrefix;

        static CloudFileHelper()
        {
            var region = ConfigurationManager.AppSettings["AWS:Region"] ?? "us-east-1";
            _bucketName = ConfigurationManager.AppSettings["AWS:S3:DocumentBucket"] ?? "";
            _keyPrefix = ConfigurationManager.AppSettings["AWS:S3:DocumentKeyPrefix"] ?? "";
            _s3Client = new AmazonS3Client(
                Amazon.RegionEndpoint.GetBySystemName(region));
        }

        /// <summary>
        /// Replaces File.ReadAllBytes - downloads an object from S3 as a byte array.
        /// The key is derived from the original local path, replacing the application
        /// root prefix with the configured S3 key prefix.
        /// </summary>
        public static byte[] ReadAllBytes(string localPath)
        {
            var key = GetS3Key(localPath);
            try
            {
                var response = _s3Client.GetObject(new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                });
                using (var memoryStream = new MemoryStream())
                {
                    response.ResponseStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"File not found in S3: {key}", ex);
            }
        }

        /// <summary>
        /// Replaces File.ReadAllText - downloads an object from S3 as a string.
        /// </summary>
        public static string ReadAllText(string localPath)
        {
            var bytes = ReadAllBytes(localPath);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Replaces File.Exists - checks if an object exists in S3.
        /// </summary>
        public static bool Exists(string localPath)
        {
            var key = GetS3Key(localPath);
            try
            {
                _s3Client.GetObjectMetadata(_bucketName, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces File.WriteAllBytes - uploads a byte array to S3.
        /// </summary>
        public static void WriteAllBytes(string localPath, byte[] data)
        {
            var key = GetS3Key(localPath);
            using (var stream = new MemoryStream(data))
            {
                _s3Client.PutObject(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = "application/octet-stream"
                });
            }
        }

        /// <summary>
        /// Replaces File.WriteAllText - uploads a string to S3.
        /// </summary>
        public static void WriteAllText(string localPath, string content)
        {
            var key = GetS3Key(localPath);
            _s3Client.PutObject(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                ContentBody = content,
                ContentType = "text/plain"
            });
        }

        /// <summary>
        /// Replaces File.AppendAllText - reads existing content, appends, and writes back.
        /// S3 does not support append; this reads the full object, appends, and writes back.
        /// </summary>
        public static void AppendAllText(string localPath, string content)
        {
            var key = GetS3Key(localPath);
            var existingContent = "";
            try
            {
                var response = _s3Client.GetObject(new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                });
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    existingContent = reader.ReadToEnd();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // File doesn't exist yet, start fresh
            }

            _s3Client.PutObject(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                ContentBody = existingContent + content,
                ContentType = "text/plain"
            });
        }

        /// <summary>
        /// Replaces File.Delete - deletes an object from S3.
        /// </summary>
        public static void Delete(string localPath)
        {
            var key = GetS3Key(localPath);
            _s3Client.DeleteObject(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            });
        }

        /// <summary>
        /// Converts a local path to an S3 key.
        /// Strips the application root prefix (~) and leading slashes,
        /// and prepends the configured key prefix.
        /// </summary>
        private static string GetS3Key(string localPath)
        {
            // Remove Server.MapPath prefix if present (paths like ~\Content\Document\file.docx)
            var path = localPath.Replace("\\", "/");
            
            // Remove leading ~ and /
            if (path.StartsWith("~/"))
            {
                path = path.Substring(2);
            }
            else if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            
            // Remove leading Content/ if present (paths are relative to app root)
            // The key prefix handles the bucket organization
            return _keyPrefix + path;
        }
    }
}
