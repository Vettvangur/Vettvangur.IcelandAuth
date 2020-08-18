using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;
using Vettvangur.IcelandAuth.Umbraco8;

namespace Vettvangur.IcelandAuth.Sample.Umbraco8.App_Start
{
    class Composer : IUserComposer
    {
        /// <summary>
        /// Umbraco lifecycle method
        /// </summary>
        public void Compose(Composition composition)
        {
            composition.Components()
                .Append<Startup>()
                ;
        }
    }

    class Startup : IComponent
    {
        protected readonly ILogger Logger;
        protected readonly IMemberService MemberService;

        public Startup(
            ILogger logger,
            IMemberService memberService
        )
        {
            Logger = logger;
            MemberService = memberService;
        }

        public void Initialize()
        {
            IcelandAuthController.Success += HandleLogin;
            IcelandAuthController.Error += HandleError;
        }

        public void Terminate() { }

        private string HandleLogin(HttpRequestBase Request, SamlLogin login)
        {
            var member = MemberService.GetByUsername(login.UserSSN);

            if (member == null)
            {
                Logger.Info<Startup>($"Creating new User: {login.UserSSN}");

                member = MemberService.CreateMemberWithIdentity(
                    login.UserSSN,
                    login.UserSSN + "@example.com",
                    login.Name,
                    "Member"
                );

                // Create member with random pw
                // This ensures users can only login using Ísland.is authentication method
                byte[] pwBytes = new byte[32];
                var rngCsp = new RNGCryptoServiceProvider();
                rngCsp.GetBytes(pwBytes);

                MemberService.AssignRole(member.Id, "Members");
                MemberService.SavePassword(member, Convert.ToBase64String(pwBytes));
                MemberService.Save(member);
            }

            // This causes all subsequent requests for the user to be 
            // authenticated as the given umbraco member
            FormsAuthentication.SetAuthCookie(login.UserSSN, true);

            // Provide a way for views and services to access the sessions saml login result
            Request.RequestContext.HttpContext.Session["samlLogin"] = login;

            // Return a custom redirect url
            return null;
        }

        private string HandleError(HttpRequestBase Request, SamlLogin login)
        {
            Logger.Error<Startup>("Error encountered while attempting Ísland.is authentication.");
            
            // Handle erronous logins here
            if (login != null)
            {

            }

            return null;
        }
    }
}
