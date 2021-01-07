using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Vettvangur.IcelandAuth.UmbracoShared;

namespace Vettvangur.IcelandAuth.Umbraco7
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        protected ILog Log;
        protected ControllerBehavior AuthHandler;

        public IcelandAuthController()
        {
            Log = LogManager.GetLogger(typeof(IcelandAuthService));
            var log = new Log4NetLogger(Log);
            var icelandAuthService = new IcelandAuthService(log);
            AuthHandler = new ControllerBehavior(icelandAuthService);
        }

        public virtual ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
