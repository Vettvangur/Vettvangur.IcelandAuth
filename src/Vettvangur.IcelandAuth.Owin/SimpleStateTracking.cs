using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    /// <summary>
    /// ToDo: Refactor to allow custom caching implementation, 
    /// current implementation does not support all load balancing / clustering scenarios.
    /// Note: Cookies do not work due to sameSite restrictions unless we always force HTTPS
    /// </summary>
    static class SimpleStateTracking
    {
        public static void StoreRedirect(
            IcelandAuthService service,
            AuthenticationResponseChallenge challenge)
        {
            if (challenge.Properties != null)
            {
                // Our implementation of island.is suggests using the AuthID
                // as a means of routing, considering it a redundant form of security verification.
                // In the case of backoffice authentication no routing should be done by library users
                // leaving it free for our usage.
                service.AuthID = Guid.NewGuid();

                // Store redirect uri
                System.Runtime.Caching.MemoryCache.Default.Set(
                    service.AuthID.ToString(),
                    challenge.Properties,
                    DateTimeOffset.Now.AddMinutes(15));
            }
        }

        public static AuthenticationProperties RetrieveRedirect(
            SamlLogin login)
        {
            var authIdResp = login.Attributes.FirstOrDefault(x => x.Name == "AuthID")?.Value;
            if (authIdResp == null)
            {
                return null;
            }

            return (AuthenticationProperties) System.Runtime.Caching.MemoryCache.Default.Get(authIdResp);
        }
    }
}
