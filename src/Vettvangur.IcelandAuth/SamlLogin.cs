using System;
using System.Collections.Generic;
using System.Text;

namespace Vettvangur.IcelandAuth
{
    public class SamlLogin
    {
        public string Name { get; internal set; }

        public string UserSSN { get; internal set; }

        /// <summary>
        /// Users mobile
        /// </summary>
        public string UserPhone { get; set; }

        public string OnbehalfRight { get; internal set; }
        public string OnBehalfName { get; internal set; }
        public string OnbehalfSSN { get; internal set; }
        public string OnbehalfValue { get; internal set; }
        public DateTime OnbehalfValidity { get; internal set; }

        /// <summary>
        /// Possible values include:
        /// "Rafræn skilríki"
        /// "Rafræn símaskilríki"
        /// "Íslykill"
        /// </summary>
        public string Authentication { get; internal set; }

        public string Message { get; internal set; }

        public bool SignatureOk { get; internal set; }
        public bool CertOk { get; internal set; }
        public bool TimeOk { get; internal set; }
        public bool AudienceOk { get; internal set; }

        public bool IpOk { get; internal set; }
        public bool AuthMethodOk { get; internal set; }

        public List<IcelandAuthAttribute> Attributes { get; } = new List<IcelandAuthAttribute>();

        public bool Valid =>
            SignatureOk && CertOk && TimeOk && IpOk && AuthMethodOk && AudienceOk;
    }
}
