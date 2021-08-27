#if NET5_0
using System;
using System.Security.Cryptography.X509Certificates;

namespace Vettvangur.IcelandAuth
{
    partial class IslandsrotCertificateChain
    {
        public static X509Certificate2 Islandsrot => new X509Certificate2(Convert.FromBase64String(IslandsrotBase64));
        public static X509Certificate2 FullgiltAudkenni => new X509Certificate2(Convert.FromBase64String(FullgiltAudkenniBase64));

    }
}
#endif
