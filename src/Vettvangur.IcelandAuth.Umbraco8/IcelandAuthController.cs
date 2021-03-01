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
        private Umbraco.Core.Logging.ILogger Log;
        private ControllerBehavior AuthHandler;

        public IcelandAuthController(
            Umbraco.Core.Logging.ILogger logger, 
            IcelandAuthService icelandAuthService = null)
        {
            Log = logger;
            if (icelandAuthService == null)
            {
                var log = new UmbracoLogger(Log, typeof(IcelandAuthService));
                icelandAuthService = new IcelandAuthService(log);
            }
            AuthHandler = new ControllerBehavior(icelandAuthService);
        }

        public ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
