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

namespace Vettvangur.IcelandAuth.UmbracoShared
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class ControllerBehavior
    {
        public static event SuccessCallback Success;
        public static event ErrorCallback Error;

        protected readonly string SuccessRedirect;
        protected readonly string ErrorRedirect;

        protected readonly IcelandAuthService IcelandAuthService;

        public ControllerBehavior(IcelandAuthService icelandAuthService)
        {
            IcelandAuthService = icelandAuthService;

            SuccessRedirect = string.IsNullOrEmpty(ConfigurationManager.AppSettings["IcelandAuth:SuccessRedirect"])
                ? "/"
                : ConfigurationManager.AppSettings["IcelandAuth:SuccessRedirect"];
            ErrorRedirect = string.IsNullOrEmpty(ConfigurationManager.AppSettings["IcelandAuth:ErrorRedirect"])
                ? "/"
                : ConfigurationManager.AppSettings["IcelandAuth:ErrorRedirect"];
        }

        public virtual string Login(HttpRequestBase request = null)
        {
            if (request == null)
            {
                request = new HttpRequestWrapper(HttpContext.Current.Request);
            }

            string callbackRedirect;

            var samlString = request["token"];

            SamlLogin login = null;
            if (!string.IsNullOrEmpty(samlString))
            {
                login = IcelandAuthService.VerifySaml(samlString, request.UserHostAddress);

                if (login?.Valid == true)
                {
                    callbackRedirect = Success?.Invoke(login);
                    return callbackRedirect ?? SuccessRedirect;
                }
            }

            callbackRedirect = Error?.Invoke(login);

            return callbackRedirect ?? ErrorRedirect;
        }
    }

    public delegate string SuccessCallback(SamlLogin login);
    public delegate string ErrorCallback(SamlLogin login);
}
