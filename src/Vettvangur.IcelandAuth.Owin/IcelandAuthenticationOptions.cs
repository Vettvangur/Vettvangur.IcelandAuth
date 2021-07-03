using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    public class IcelandAuthenticationOptions : AuthenticationOptions
    {
        public IcelandAuthenticationOptions()
            : this(IcelandAuthenticationDefaults.AuthenticationType)
        { }

        public IcelandAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {

        }

        /// <summary>
        /// Override default configuration for the IcelandAuthService used to process
        /// island.is Saml responses.
        /// <para>Do not set the AuthID property, it is used by the authentication handler for post redirect state retrieval.</para>
        /// </summary>
        public Func<IOwinContext, IcelandAuthService> IcelandAuthService { get; set; }

        public PathString CallbackPath { get; set; }
        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By Design")]
        public string RedirectUri { get; set; }

        private string _signInAsAuthenticationType;
        /// <summary>
        /// Gets or sets the AuthenticationType used when creating the <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType
        {
            get { return _signInAsAuthenticationType; }
            set { _signInAsAuthenticationType = value; }
        }
    }
}
