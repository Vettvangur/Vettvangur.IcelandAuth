#if NET5
using System.Security.Cryptography.X509Certificates;

namespace Vettvangur.IcelandAuth
{
    partial class AudkenniCertificateChain
    {
        public static X509Certificate2 Audkennisrot => new X509Certificate2(AudkennisrotBytes);
        public static X509Certificate2 TrausturBunadur => new X509Certificate2(TrausturBunadurBytes);
        public static X509Certificate2 TraustAudkenni => new X509Certificate2(TraustAudkenniBytes);
    }
}
#endif
