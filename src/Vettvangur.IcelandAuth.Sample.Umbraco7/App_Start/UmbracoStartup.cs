using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web;
using Vettvangur.IcelandAuth.Umbraco7;

namespace Vettvangur.IcelandAuth.Sample.Umbraco7.App_Start
{
    class UmbracoStartup : ApplicationEventHandler
    {
        protected static readonly ILog Logger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType
        );

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            IcelandAuthController.SuccessCallback += HandleLogin;
            IcelandAuthController.ErrorCallback += HandleError;
        }

        private string HandleLogin(SamlLogin login, HttpRequestBase Request)
        {
            var ms = ApplicationContext.Current.Services.MemberService;

            var member = ms.GetByUsername(login.UserSSN);

            if (member == null)
            {
                Logger.Info($"Creating new User: {login.UserSSN}");

                member = ms.CreateMemberWithIdentity(
                    login.UserSSN,
                    login.UserSSN + "@example.com",
                    login.Name,
                    "Member"
                );

                // Create member with random pw
                byte[] pwBytes = new byte[32];
                var rngCsp = new RNGCryptoServiceProvider();
                rngCsp.GetBytes(pwBytes);

                ms.AssignRole(member.Id, "Members");
                ms.SavePassword(member, Convert.ToBase64String(pwBytes));
                ms.Save(member);
            }

            FormsAuthentication.SetAuthCookie(login.UserSSN, true);

            Request.RequestContext.HttpContext.Session["login"] = login;
            return null;
        }

        private string HandleError(HttpRequestBase Request)
        {
            return null;
        }
    }
}
