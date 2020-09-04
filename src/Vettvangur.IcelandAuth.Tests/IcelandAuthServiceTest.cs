using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Vettvangur.IcelandAuth.Tests
{
    [TestClass]
    public class IcelandAuthServiceTest
    {
        IcelandAuthService svc;
        [TestInitialize]
        public void Init()
        {
            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Audience", "icelandauth.vettvangur.is");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Destination", "https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:DestinationSSN", "5208130550");

            svc = new IcelandAuthService(configuration.Object, null);
        }

        [TestMethod]
        public void VerifiesOnceValidSaml()
        {
            var login = svc.VerifySaml(Resources.OnetimeValidSaml, "194.144.213.209");

            // is in the past
            Assert.IsFalse(login.TimeOk);
            Assert.IsFalse(login.Valid);

            Assert.IsTrue(login.CertOk);
            Assert.IsTrue(login.DestinationOk);
            Assert.IsTrue(login.DestinationSsnOk);
            Assert.IsTrue(login.IpOk);
            Assert.IsTrue(login.AuthMethodOk);
            Assert.IsTrue(login.AudienceOk);

            // Ignored since not configured
            Assert.IsTrue(login.AuthIdOk);

            Assert.AreEqual("2008862919", login.UserSSN);
            Assert.AreEqual("Íslykill", login.Authentication);
        }

        /// <summary>
        /// Requires audkennisrot in trusted roots
        /// </summary>
        [TestMethod]
        public void VerifiesSignature()
        {
            var login = svc.VerifySaml(Resources.OnetimeValidSaml, null);

            Assert.IsTrue(login.SignatureOk);
        }
    }
}
