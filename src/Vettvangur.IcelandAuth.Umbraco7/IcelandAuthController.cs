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

namespace Vettvangur.IcelandAuth.Umbraco7
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class IcelandAuthController : SurfaceController
    {
        public static event SuccessCallback SuccessCallback;
        public static event ErrorCallback ErrorCallback;

        protected readonly string SuccessRedirect;
        protected readonly string ErrorRedirect;

        protected readonly IcelandAuthService IcelandAuthService;
        protected readonly ILog Log;


        public IcelandAuthController()
        {
            Log = LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType
                );
            var log = new Log4NetLogger(Log);
            IcelandAuthService = new IcelandAuthService(log);

            SuccessRedirect = ConfigurationManager.AppSettings["IcelandAuth.SuccessRedirect"] ?? "/";
            ErrorRedirect = ConfigurationManager.AppSettings["IcelandAuth.ErrorRedirect"] ?? "/";
        }

        public virtual ActionResult Login()
        {
            string callbackRedirect;

            var samlString = Request["token"];

            SamlLogin login = null;
            if (!string.IsNullOrEmpty(samlString))
            {
                login = IcelandAuthService.VerifySaml(samlString, Request.UserHostAddress);

                if (login?.Valid == true)
                {
                    callbackRedirect = SuccessCallback?.Invoke(Request, login);
                    return Redirect(callbackRedirect ?? SuccessRedirect);
                }
            }

            callbackRedirect = ErrorCallback?.Invoke(Request, login);

            return Redirect(callbackRedirect ?? ErrorRedirect);
        }
    }

    public delegate string SuccessCallback(HttpRequestBase Request, SamlLogin login);
    public delegate string ErrorCallback(HttpRequestBase Request, SamlLogin login);
}
