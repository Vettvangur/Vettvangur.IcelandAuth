using IdentityModel;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    public class IcelandAuthenticationHandler : AuthenticationHandler<IcelandAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public IcelandAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles SignIn
        /// </summary>
        /// <returns></returns>
        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
                if (challenge == null)
                {
                    return Task.CompletedTask;
                }

                var svc = Options.IcelandAuthService(Context);
                SimpleStateTracking.StoreRedirect(svc, challenge);

                var redirectUrl = svc.CreateLoginUrl();
                Response.Redirect(redirectUrl);
            }

            return Task.CompletedTask;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            SamlLogin login = null;

            if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrWhiteSpace(Request.ContentType)
              // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
            {
                if (!Request.Body.CanSeek)
                {
                    _logger.WriteVerbose("Buffering request body");
                    // Buffer in case this body was not meant for us.
                    MemoryStream memoryStream = new MemoryStream();
                    await Request.Body.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Request.Body = memoryStream;
                }

                IFormCollection form = await Request.ReadFormAsync();
                Request.Body.Seek(0, SeekOrigin.Begin);

                var token = form.Get("token");
                if (token != null)
                {
                    login = Options.IcelandAuthService(Context).VerifySaml(token, Request.RemoteIpAddress);
                }
            }

            if (login?.Valid == true)
            {
                // Identity authenticationType "UmbracoExternalCookie"
                // Not to be confused with the issuer provided below from Options.AuthenticationType
                var claimsIdentity = new ClaimsIdentity(Options.SignInAsAuthenticationType);
                //var claimsIdentity = new ClaimsIdentity(Options.AuthenticationType);
                AddClaimsFromSaml(claimsIdentity, login);

                var properties = SimpleStateTracking.RetrieveRedirect(login);

                return new AuthenticationTicket(
                    claimsIdentity,
                    properties
                );
            }

            return null;
        }

        private void AddClaimsFromSaml(ClaimsIdentity claimsIdentity, SamlLogin samlLogin)
        {
            claimsIdentity.AddClaim(new Claim(
                ClaimTypes.Name,
                samlLogin.Name,
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );
            claimsIdentity.AddClaim(new Claim(
                ClaimTypes.NameIdentifier,
                samlLogin.UserSSN,
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );
            claimsIdentity.AddClaim(new Claim(
                JwtClaimTypes.Issuer,
                Options.AuthenticationType,
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );
            claimsIdentity.AddClaim(new Claim(
                ClaimTypes.Email,
                $"{samlLogin.UserSSN}@example.com",
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );
            claimsIdentity.AddClaim(new Claim(
                ClaimTypes.Upn,
                samlLogin.UserSSN,
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );
            claimsIdentity.AddClaim(new Claim(
                ClaimTypes.AuthenticationMethod,
                samlLogin.Authentication,
                ClaimValueTypes.String,
                Options.AuthenticationType)
            );

            if (!string.IsNullOrEmpty(samlLogin.UserPhone))
            {
                claimsIdentity.AddClaim(new Claim(
                    ClaimTypes.MobilePhone,
                    samlLogin.UserPhone,
                    ClaimValueTypes.String,
                    Options.AuthenticationType)
                );
            }
            var ipAddress = samlLogin.Attributes.FirstOrDefault(x => x.Name == "IPAddress");
            if (ipAddress != null)
            {
                claimsIdentity.AddClaim(new Claim(
                    "ipaddr",
                    ipAddress.Value,
                    ClaimValueTypes.String,
                    Options.AuthenticationType)
                );
            }
        }

        /// <summary>
        /// Calls InvokeReplyPathAsync
        /// </summary>
        /// <returns>True if the request was handled, false if the next middleware should be invoked.</returns>
        public override async Task<bool> InvokeAsync()
        {
            if (Options.CallbackPath.HasValue
            && Options.CallbackPath != Request.PathBase + Request.Path)
            {
                return false;
            }

            AuthenticationTicket ticket = await AuthenticateAsync();

            if (ticket != null)
            {
                if (ticket.Identity != null)
                {
                    Request.Context.Authentication.SignIn(ticket.Properties, ticket.Identity);
                }
                // Redirect back to the original secured resource, if any.
                if (!string.IsNullOrWhiteSpace(ticket.Properties.RedirectUri))
                {
                    Response.Redirect(ticket.Properties.RedirectUri);
                    return true;
                }
            }

            return false;
        }
    }
}
