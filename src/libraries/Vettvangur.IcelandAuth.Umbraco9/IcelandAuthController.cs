using System;
using System.Collections.Generic;
using System.Linq;
using Vettvangur.IcelandAuth.UmbracoShared;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Umbraco9
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        private ControllerBehaviorCore AuthHandler;
        public IcelandAuthController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IcelandAuthService icelandAuthService,
            IConfiguration configuration)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            AuthHandler = new ControllerBehaviorCore(icelandAuthService, configuration);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Login()
        {
            return Redirect(await AuthHandler.Login(Request));
        }
    }
}
