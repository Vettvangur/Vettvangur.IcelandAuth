using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    /// <summary>
    /// Extension methods for using <see cref="IcelandAuthenticationMiddleware"/>
    /// </summary>
    public static class IcelandAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="IcelandAuthenticationMiddleware"/> into the OWIN runtime.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="icelandAuthOptions">A <see cref="IcelandAuthenticationOptions"/> contains settings for obtaining identities using island.is authentication services Saml tokens.</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseIcelandAuthentication(
            this IAppBuilder app,
            IcelandAuthenticationOptions icelandAuthOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (icelandAuthOptions == null)
            {
                throw new ArgumentNullException(nameof(icelandAuthOptions));
            }

            return app.Use(typeof(IcelandAuthenticationMiddleware), app, icelandAuthOptions);
        }
    }
}
