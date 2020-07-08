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

namespace Vettvangur.IcelandAuth.Umbraco8
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
        protected readonly Umbraco.Core.Logging.ILogger Log;


        public IcelandAuthController()
        {
            Log = Current.Logger;
            var log = new UmbracoLogger(Log, typeof(IcelandAuthController));
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
                Log.Error<IcelandAuthController>(ex);
            }

            callbackRedirect = ErrorCallback?.Invoke(Request);

            return Redirect(callbackRedirect ?? ErrorRedirect);
        }
    }

    public delegate string SuccessCallback(SamlLogin login, HttpRequestBase Request);
    public delegate string ErrorCallback(HttpRequestBase Request);
}
