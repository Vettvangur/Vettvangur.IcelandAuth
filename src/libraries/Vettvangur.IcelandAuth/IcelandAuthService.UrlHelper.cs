using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth
{
    public partial class IcelandAuthService
    {
#if NETFRAMEWORK
        /// <summary>
        /// Creates an island.is login url, reading key/values from <see cref="ConfigurationManager.AppSettings"/>.
        /// </summary>
        /// <param name="lang">
        /// Allows configuration of display language in island.is portal. <br />
        /// Two-letter country code as defined in ISO 3166-1
        /// </param>
        /// <returns></returns>
        public static string CreateUrl(
            string lang = null,
            IEnumerable<string> authentication = null,
            Guid? authId = null)
        {
            var svc = new IcelandAuthService(null);
            if (authentication != null)
            {
                svc.Authentication = authentication;
            }
            if (authId != null)
            {
                svc.AuthID = authId;
            }

            return svc.CreateLoginUrl(lang);
        }
#endif

        /// <summary>
        /// Create an island.is login url from the provided parameters
        /// </summary>
        /// <param name="islandIsId">Id used by island.is to define separate destination url registrations for a registrant</param>
        /// <param name="lang">
        /// Allows configuration of display language in island.is portal. <br />
        /// Two-letter country code as defined in ISO 3166-1
        /// </param>
        /// <param name="authentication">
        /// Rafræn skilríki – Digital certificate authentication. <br />
        /// Rafræn símaskilríki - Digital certificate authentication using a phone. <br />
        /// Rafræn starfsmannaskilríki – Employee digital certificate authentication. <br />
        /// Íslykill – Authentication using Íslykill. <br />
        /// Styrktur Íslykill – 2FA using Íslykill, 2FA delivered via phone or email. <br />
        /// Styrkt rafræn skilríki – Digital certificate authentication with 2FA via phone/email. <br />
        /// Styrkt rafræn starfsmannaskilríki – Employee digital certificate authentication with 2FA via phone/email. <br />
        /// </param>
        /// <param name="authId">
        /// An optional Guid that will be sent along with the token to the configured destination url
        /// </param>
        /// <returns></returns>
#if NETFRAMEWORK
        public static string CreateUrlWithId(
#else
        public static string CreateUrl(
#endif
            string islandIsId,
            string lang = null,
            IEnumerable<string> authentication = null, 
            Guid? authId = null)
        {
            var svc = new IcelandAuthService(null, null);
            svc.IslandIsID = islandIsId;
            svc.Authentication = authentication;
            svc.AuthID = authId;

            return svc.CreateLoginUrl(lang);
        }

        const string _islandIsUrl = "https://innskraning.island.is/";

        /// <summary>
        /// island.is ID - Required for url helper
        /// </summary>
        public virtual string IslandIsID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang">
        /// Allows configuration of display language in island.is portal. <br />
        /// Two-letter country code as defined in ISO 3166-1
        /// </param>
        /// <returns></returns>
        public string CreateLoginUrl(string lang = null)
        {
            if (string.IsNullOrEmpty(IslandIsID))
            {
                throw new InvalidOperationException(
                    $"Missing {nameof(IslandIsID)}, either configure in app settings or set the service property."
                );
            }

            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("id", IslandIsID);

            if (!string.IsNullOrEmpty(lang))
            {
                if (lang.Length != 2)
                {
                    throw new ArgumentException(
                        "Please use a two-letter country code (ISO 3166-1 alpha-2)", 
                        nameof(lang));
                }

                queryString.Add("lang", lang);
            }

            if (Authentication?.Any() == true)
            {
                // Don't offer íslykill authentication in UI if it is not allowed in Authentication constraints
                if (!Authentication.Any(x => x.ToUpper().Contains("ÍSLYKILL")))
                {
                    queryString.Add("qaa", "4");
                }
                // 2FA and non-2FA authentication methods are incompatible with the current UI of island.is.
                // We default to 2FA UI if Authentication constraint includes at least one 2FA auth method.
                else if (Authentication.Any(x => x.ToUpper().Contains("STYRKT")))
                {
                    queryString.Add("qaa", "3");
                }
            }

            if (AuthID != null)
            {
                queryString.Add("authid", AuthID.Value.ToString());
            }

            return $"{_islandIsUrl}?{queryString}";
        }
    }
}
