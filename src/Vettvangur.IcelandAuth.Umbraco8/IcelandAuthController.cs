using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Vettvangur.IcelandAuth.UmbracoShared;

namespace Vettvangur.IcelandAuth.Umbraco8
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        protected Umbraco.Core.Logging.ILogger Log;
        protected ControllerBehavior AuthHandler;

        public IcelandAuthController(Umbraco.Core.Logging.ILogger logger)
        {
            Log = logger;
            var log = new UmbracoLogger(Log, typeof(IcelandAuthService));
            var icelandAuthService = new IcelandAuthService(log);
            AuthHandler = new ControllerBehavior(icelandAuthService);
        }

        public virtual ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
