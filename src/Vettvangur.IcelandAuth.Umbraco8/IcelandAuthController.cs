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
        private ControllerBehavior AuthHandler;

        public IcelandAuthController(
            Umbraco.Core.Logging.ILogger logger, 
            IcelandAuthService icelandAuthService)
        {
            AuthHandler = new ControllerBehavior(icelandAuthService);
        }

        public ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
