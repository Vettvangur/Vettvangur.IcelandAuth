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
        public static string CreateUrl(IEnumerable<string> authentication = null, Guid? authId = null)
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

            return svc.CreateLoginUrl();
        }
#endif

        public static string CreateUrl(
            string islandIsId, 
            IEnumerable<string> authentication = null, 
            Guid? authId = null)
        {
            var svc = new IcelandAuthService(null, null);
            svc.IslandIsID = islandIsId;
            svc.Authentication = authentication;
            svc.AuthID = authId;

            return svc.CreateLoginUrl();
        }

        const string _islandIsUrl = "https://innskraning.island.is/";

        /// <summary>
        /// island.is ID - Required for url helper
        /// </summary>
        public virtual string IslandIsID { get; set; }

        public string CreateLoginUrl()
        {
            if (string.IsNullOrEmpty(IslandIsID))
            {
                throw new InvalidOperationException(
                    $"Missing {nameof(IslandIsID)}, either configure in app settings or set the service property."
                );
            }

            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("id", IslandIsID);
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
