using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;
using Umbraco.Web.Mvc;
using Vettvangur.IcelandAuth.UmbracoShared;

namespace Vettvangur.IcelandAuth.Umbraco8
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        protected readonly Umbraco.Core.Logging.ILogger Log;
        protected readonly ControllerBehavior AuthHandler;

        public IcelandAuthController(Umbraco.Core.Logging.ILogger logger)
        {
            Log = logger;
            var log = new UmbracoLogger(Log, typeof(IcelandAuthService));
            var icelandAuthService = new IcelandAuthService(log);
            AuthHandler = new ControllerBehavior(Request, icelandAuthService);
        }

        public virtual ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
