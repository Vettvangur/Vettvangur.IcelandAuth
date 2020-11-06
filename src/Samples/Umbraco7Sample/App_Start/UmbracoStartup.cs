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
using Vettvangur.IcelandAuth;
using Vettvangur.IcelandAuth.UmbracoShared;

namespace Umbraco7Sample.App_Start
{
    class UmbracoStartup : ApplicationEventHandler
    {
        protected static readonly ILog Logger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType
        );

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ControllerBehavior.Success += HandleLogin;
            ControllerBehavior.Error += HandleError;
        }

        private static string HandleLogin(SamlLogin login)
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
                // This ensures users can only login using Ísland.is authentication method
                byte[] pwBytes = new byte[32];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetBytes(pwBytes);
                }

                ms.AssignRole(member.Id, "Members");
                ms.SavePassword(member, Convert.ToBase64String(pwBytes));
                ms.Save(member);
            }

            // This causes all subsequent requests for the user to be 
            // authenticated as the given umbraco member
            FormsAuthentication.SetAuthCookie(login.UserSSN, true);

            // Provide a way for views and services to access the sessions saml login result
            HttpContext.Current.Session["login"] = login;

            // Return a custom redirect url
            return null;
        }

        private static string HandleError(SamlLogin login)
        {
            Logger.Error("Error encountered while attempting Ísland.is authentication.");

            // Handle erronous logins here
            if (login != null)
            {

            }

            return null;
        }
    }
}
