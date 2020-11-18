using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vettvangur.IcelandAuth
{
    public class SamlLogin
    {
        public string Name => Attributes.FirstOrDefault(x => x.Name == "Name")?.Value;

        public string UserSSN => Attributes.FirstOrDefault(x => x.Name == "UserSSN")?.Value;

        /// <summary>
        /// Users mobile
        /// </summary>
        public string UserPhone => Attributes.FirstOrDefault(x => x.Name == "Mobile")?.Value;

        public string UserAgent => Attributes.FirstOrDefault(x => x.Name == "UserAgent")?.Value;

        public string OnbehalfRight => Attributes.FirstOrDefault(i => i.Name == "BehalfRight")?.Value;
        public string OnBehalfName => Attributes.FirstOrDefault(i => i.Name == "OnBehalfName")?.Value;
        public string OnbehalfSSN => Attributes.FirstOrDefault(i => i.Name == "OnBehalfUserSSN")?.Value;
        public string OnbehalfValue => Attributes.FirstOrDefault(i => i.Name == "BehalfValue")?.Value;
        public DateTime? OnbehalfValidity
        {
            get
            {
                var behalfValidityAttr = Attributes.FirstOrDefault(i => i.Name == "BehalfValidity");
                if (DateTime.TryParse(behalfValidityAttr?.Value, out var val))
                {
                    return val;
                }

                return null;
            }
        }

        /// <summary>
        /// Possible values include:
        /// "Rafræn skilríki"
        /// "Rafræn símaskilríki"
        /// "Íslykill"
        /// </summary>
        public IEnumerable<string> Authentication
        {
            get
            {
                var val = Attributes.FirstOrDefault(x => x.Name == "Authentication")?.Value;

                if (val != null)
                {
                    return val.Split(',').Select(x => x.Trim(' '));
                }

                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Client IP Address
        /// </summary>
        public string IPAddress => Attributes.FirstOrDefault(x => x.Name == "IPAddress")?.Value;

        public Guid? AuthID
        {
            get
            {
                var val = Attributes.FirstOrDefault(x => x.Name == "AuthID")?.Value;

                if (Guid.TryParse(val, out var key))
                {
                    return key;
                }

                return null;
            }
        }

        public bool SignatureOk { get; set; }
        public bool CertOk { get; set; }
        public bool TimeOk { get; set; }
        public bool AudienceOk { get; set; }

        public bool IpOk { get; set; }
        public bool AuthMethodOk { get; set; }
        public bool DestinationOk { get; set; }
        public bool DestinationSsnOk { get; set; }
        public bool AuthIdOk { get; set; }

        public List<IcelandAuthAttribute> Attributes { get; } = new List<IcelandAuthAttribute>();

        /// <summary>
        /// Returns true if all validation is successful
        /// </summary>
        public bool Valid =>
            SignatureOk && CertOk && TimeOk && IpOk && AuthMethodOk && AudienceOk && AuthIdOk && DestinationOk && DestinationSsnOk;
    }
}
