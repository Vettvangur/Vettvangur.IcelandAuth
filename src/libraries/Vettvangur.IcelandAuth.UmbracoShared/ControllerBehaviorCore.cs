using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Vettvangur.IcelandAuth.UmbracoShared
{
    /// <summary>
    /// /umbraco/surface/icelandAuth/Login
    /// </summary>
    public class ControllerBehaviorCore
    {
        public static event SuccessCallback Success;
        public static event ErrorCallback Error;

        protected readonly string SuccessRedirect;
        protected readonly string ErrorRedirect;

        protected readonly IcelandAuthService IcelandAuthService;

        public ControllerBehaviorCore(IcelandAuthService icelandAuthService, IConfiguration configuration)
        {
            IcelandAuthService = icelandAuthService;

            SuccessRedirect 
                = string.IsNullOrEmpty(configuration["IcelandAuth:SuccessRedirect"])
                ? "/"
                : configuration["IcelandAuth:SuccessRedirect"];
            ErrorRedirect 
                = string.IsNullOrEmpty(configuration["IcelandAuth:ErrorRedirect"])
                ? "/"
                : configuration["IcelandAuth:ErrorRedirect"];
        }

        public virtual async Task<string> Login(HttpRequest request)
        {
            string callbackRedirect;

            var samlString = request.Form["token"];

            SamlLogin login = null;
            if (!string.IsNullOrEmpty(samlString))
            {
                login = IcelandAuthService.VerifySaml(
                    samlString, 
                    request.HttpContext.Connection.RemoteIpAddress.ToString());

                if (login?.Valid == true)
                {
                    callbackRedirect = await Success?.Invoke(login);
                    return callbackRedirect ?? SuccessRedirect;
                }
            }

            callbackRedirect = await Error?.Invoke(login);

            return callbackRedirect ?? ErrorRedirect;
        }
    }

    public delegate Task<string> SuccessCallback(SamlLogin login);
    public delegate Task<string> ErrorCallback(SamlLogin login);
}
