using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vettvangur.IcelandAuth.Owin;
using Umbraco.Core;
using Umbraco.Web.Security;
using Vettvangur.IcelandAuth;
using Umbraco.Web.Security.Identity;
using Vettvangur.IcelandAuth.Umbraco7;
using log4net;

namespace Umbraco7Sample.App_Start
{
    public static class UmbracoIcelandAuthExtensions
    {
        ///  <summary>
        ///  Configure island.is sign-in
        ///  
        /// ToDo: Install with owin nupkg
        /// 
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="postLoginRedirectUri">
        ///  The URL that will be redirected to after login is successful, example: http://mydomain.com/umbraco/;
        ///  </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        public static void ConfigureBackOfficeIcelandAuth(
            this IAppBuilder app,
            string postLoginRedirectUri,
            string caption = "island.is",
            string style = "btn-yahoo",
            string icon = "fa-flag")
        {
            var options = new IcelandAuthenticationOptions
            {
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                RedirectUri = postLoginRedirectUri
            };

            options.ForUmbracoBackOffice(style, icon);
            options.Description.Caption = caption;

            // Fix auth type since ForUmbracoBackOffice overrides
            // See https://issues.umbraco.org/issue/U4-7121
            options.AuthenticationType = "https://innskraning.island.is/?id=test.icelandauth.vettvangur.is";

            // Never enable auto-linking with island.is unless you want to grant all
            // icelandic nationals access to your backoffice

            //var autoLinkOptions = new ExternalSignInAutoLinkOptions(
            //  autoLinkExternalAccount: true,
            //  defaultUserGroups: new[] { "admin" },
            //  defaultCulture: "en-US")
            //{
            //    OnExternalLogin = (backofficeUser, externalLogin) => true,
            //};

            //options.SetExternalSignInAutoLinkOptions(autoLinkOptions);

            var log = LogManager.GetLogger(typeof(IcelandAuthService));
            var logger = new Log4NetLogger(log);
            options.IcelandAuthService = ctx => new IcelandAuthService(logger)
            {
                IslandIsID = "test.icelandauth.vettvangur.is",
                Destination = postLoginRedirectUri,
                AuthID = null,
            };
            app.UseIcelandAuthentication(options);
        }
    }
}
