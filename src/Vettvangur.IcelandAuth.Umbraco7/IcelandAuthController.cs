using log4net;
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
using Umbraco.Web.Mvc;
using Vettvangur.IcelandAuth.Umbraco7.Log4NetCompat;
using Vettvangur.IcelandAuth.UmbracoShared;

namespace Vettvangur.IcelandAuth.Umbraco7
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        protected readonly string SuccessRedirect;
        protected readonly string ErrorRedirect;

        protected readonly ControllerBehavior AuthHandler;
        protected readonly ILog Log;

        public IcelandAuthController()
        {
            Log = LogManager.GetLogger(typeof(IcelandAuthService));
            var log = new Log4NetLogger(Log);
            var icelandAuthService = new IcelandAuthService(log);
            AuthHandler = new ControllerBehavior(Request, icelandAuthService);
        }

        public virtual ActionResult Login()
        {
            return Redirect(AuthHandler.Login());
        }
    }
}
