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
    /// /umbraco/icelandAuth/icelandAuth/Login
    /// </summary>
    [PluginController("IcelandAuth")]
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

            try
            {
                if (!string.IsNullOrEmpty(samlString))
                {
                    var login = IcelandAuthService.VerifySaml(samlString, Request.UserHostAddress);

                    if (login != null)
                    {
                        callbackRedirect = SuccessCallback?.Invoke(login, Request);
                        return Redirect(callbackRedirect ?? SuccessRedirect);
                    }
                }
            }
            catch(Exception ex) when (ex is FormatException || ex is XmlException || ex is System.Security.Cryptography.CryptographicException)
            {
                Log.Error(ex);
            }

            callbackRedirect = ErrorCallback?.Invoke(Request);

            return Redirect(callbackRedirect ?? ErrorRedirect);
        }
    }

    public delegate string SuccessCallback(SamlLogin login, HttpRequestBase Request);
    public delegate string ErrorCallback(HttpRequestBase Request);
}
