using System;
using System.Collections.Generic;
using System.Text;

namespace Vettvangur.IcelandAuth
{
    public class SamlLogin
    {
        public string Name { get; set; }

        public string UserSSN { get; set; }

        /// <summary>
        /// Users mobile
        /// </summary>
        public string UserPhone { get; set; }

        public string OnbehalfRight { get; set; }
        public string OnBehalfName { get; set; }
        public string OnbehalfSSN { get; set; }
        public string OnbehalfValue { get; set; }
        public DateTime OnbehalfValidity { get; set; }

        /// <summary>
        /// Possible values include:
        /// "Rafræn skilríki"
        /// "Rafræn símaskilríki"
        /// "Íslykill"
        /// </summary>
        public string Authentication { get; set; }

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
