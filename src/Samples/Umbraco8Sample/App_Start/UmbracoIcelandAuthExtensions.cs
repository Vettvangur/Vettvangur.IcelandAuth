using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vettvangur.IcelandAuth.Owin;
using Umbraco.Core;
using Umbraco.Web.Security;
using Vettvangur.IcelandAuth;
using Vettvangur.IcelandAuth.Umbraco8;
using Umbraco.Web.Composing;

namespace Umbraco8Sample.App_Start
{
    public static class UmbracoIcelandAuthExtensions
    {
        ///  <summary>
        ///  Configure ActiveDirectory sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="tenant">
        ///  Your tenant ID i.e. YOURDIRECTORYNAME.onmicrosoft.com OR this could be the GUID of your tenant ID
        ///  </param>
        ///  <param name="clientId">
        ///  Also known as the Application Id in the azure portal
        ///  </param>
        ///  <param name="postLoginRedirectUri">
        ///  The URL that will be redirected to after login is successful, example: http://mydomain.com/umbraco/;
        ///  </param>
        ///  <param name="issuerId">
        /// 
        ///  This is the "Issuer Id" for you Azure AD application. This is a GUID value of your tenant ID.
        /// 
        ///  If this value is not set correctly then accounts won't be able to be detected 
        ///  for un-linking in the back office. 
        /// 
        ///  </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        /// 
        ///  ActiveDirectory account documentation for ASP.Net Identity can be found:
        ///  https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
        /// 
        ///  </remarks>
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

            var logger = new UmbracoLogger(Current.Logger, typeof(IcelandAuthService));
            options.IcelandAuthService = ctx => new IcelandAuthService(logger)
            {
                IslandIsID = "dev.icelandauth.vettvangur.is",
                Destination = postLoginRedirectUri,
                AuthID = null,
            };
            app.UseIcelandAuthentication(options);
        }
    }
}
