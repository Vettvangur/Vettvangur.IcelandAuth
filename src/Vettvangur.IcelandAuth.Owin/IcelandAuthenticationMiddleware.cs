using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    public class IcelandAuthenticationMiddleware : AuthenticationMiddleware<IcelandAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public IcelandAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, IcelandAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<IcelandAuthenticationMiddleware>();

            if (options.IcelandAuthService == null)
            {
                var iceAuthSvcLogger = app.CreateLogger<IcelandAuthService>();

                var loggingBridge = new LoggingBridge(iceAuthSvcLogger);

                var svc = new IcelandAuthService(loggingBridge);

                options.IcelandAuthService = ctx => svc;
            }

            // if the user has not set the AuthorizeCallback, set it from the redirect_uri
            if (!Options.CallbackPath.HasValue)
            {
                if (!string.IsNullOrEmpty(Options.RedirectUri) 
                && Uri.TryCreate(Options.RedirectUri, UriKind.Absolute, out Uri redirectUri))
                {
                    // Redirect_Uri must be a very specific, case sensitive value, so we can't generate it. Instead we generate AuthorizeCallback from it.
                    Options.CallbackPath = PathString.FromUriComponent(redirectUri);
                }
            }


        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="OpenIdConnectAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<IcelandAuthenticationOptions> CreateHandler()
        {
            return new IcelandAuthenticationHandler(_logger);
        }
    }
}
