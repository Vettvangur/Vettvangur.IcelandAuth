#if NET5
using System;
using System.Security.Cryptography.X509Certificates;

namespace Vettvangur.IcelandAuth
{
    partial class AudkenniCertificateChain
    {
        public static X509Certificate2 Audkennisrot => new X509Certificate2(Convert.FromBase64String(AudkennisrotBase64));
        public static X509Certificate2 TrausturBunadur => new X509Certificate2(Convert.FromBase64String(TrausturBunadurBase64));
        public static X509Certificate2 TraustAudkenni => new X509Certificate2(Convert.FromBase64String(TraustAudkenniBase64));
    }
}
#endif
