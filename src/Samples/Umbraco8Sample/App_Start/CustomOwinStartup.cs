using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Umbraco.Web;
using Umbraco8Sample.App_Start;

[assembly: OwinStartup("CustomOwinStartup", typeof(CustomOwinStartup))]

namespace Umbraco8Sample.App_Start
{
    public class CustomOwinStartup : UmbracoDefaultOwinStartup
    {
        protected override void ConfigureUmbracoAuthentication(IAppBuilder app)
        {
            base.ConfigureUmbracoAuthentication(app);
            app.ConfigureBackOfficeIcelandAuth(
                postLoginRedirectUri: ConfigurationManager.AppSettings["IcelandAuth:Owin:RedirectUrl"]);
        }
    }
}
