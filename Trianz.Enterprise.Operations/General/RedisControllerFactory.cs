using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Custom controller factory that injects a Redis-backed TempDataProvider
    /// into every controller, replacing the default in-memory TempData provider.
    /// This ensures TempData state persists across horizontally scaled instances
    /// behind a load balancer using Amazon ElastiCache for Redis.
    /// </summary>
    public class RedisControllerFactory : DefaultControllerFactory
    {
        private readonly RedisTempDataProvider _tempDataProvider;

        public RedisControllerFactory()
        {
            _tempDataProvider = new RedisTempDataProvider();
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            var controller = base.CreateController(requestContext, controllerName) as Controller;
            if (controller != null)
            {
                controller.TempDataProvider = _tempDataProvider;
            }
            return controller;
        }
    }
}
