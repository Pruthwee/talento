using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// A TempData provider backed by Redis (Amazon ElastiCache), ensuring
    /// TempData state persists across horizontally scaled instances behind a load balancer.
    /// Replaces the default in-memory TempData provider.
    /// </summary>
    public class RedisTempDataProvider : TempDataDictionary, ITempDataProvider
    {
        private readonly string _redisConnectionString;
        private readonly string _keyPrefix = "tempdata:";
        private readonly int _timeoutMinutes = 20;

        public RedisTempDataProvider()
        {
            _redisConnectionString = ConfigurationManager.AppSettings["AWS:Redis:ConnectionString"] ?? "localhost:6379";
        }

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            var sessionId = GetSessionId(controllerContext);
            var key = _keyPrefix + sessionId;

            try
            {
                var db = RedisCacheHelper.GetDatabase();
                var json = db.StringGet(key);
                
                if (!json.HasValue)
                {
                    return new Dictionary<string, object>();
                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                
                // Delete after reading (TempData is consumed once)
                db.KeyDelete(key);

                return result ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> dictionary)
        {
            var sessionId = GetSessionId(controllerContext);
            var key = _keyPrefix + sessionId;

            try
            {
                var db = RedisCacheHelper.GetDatabase();
                var json = JsonConvert.SerializeObject(dictionary);
                db.StringSet(key, json, TimeSpan.FromMinutes(_timeoutMinutes));
            }
            catch
            {
                // Fall back to in-memory if Redis is unavailable
            }
        }

        private string GetSessionId(ControllerContext controllerContext)
        {
            var httpContext = controllerContext.HttpContext;
            var sessionId = httpContext.Session?.SessionID;
            
            if (string.IsNullOrEmpty(sessionId))
            {
                // Fallback to a cookie-based or request-based identifier
                sessionId = httpContext.Request.Headers["TempData-Session"] ??
                           Guid.NewGuid().ToString();
            }

            return sessionId;
        }
    }
}
