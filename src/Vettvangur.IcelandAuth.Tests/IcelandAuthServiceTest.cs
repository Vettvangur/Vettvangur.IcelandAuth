 using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

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
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Destination", "https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:DestinationSSN", "5208130550");

#if NETFRAMEWORK
            svc = new IcelandAuthService();
            svc.Destination = "https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login";
            svc.DestinationSSN = "5208130550";
#elif NETCOREAPP
            svc = new IcelandAuthService(configuration.Object, null, null);
#endif
        }

        [TestMethod]
        public void ThrowsOnMissingDestination()
        {
#if NETFRAMEWORK
            svc = new IcelandAuthService();
#elif NETCOREAPP
            svc = new IcelandAuthService(null, null, null);
#endif

            Assert.ThrowsException<InvalidOperationException>(() => svc.VerifySaml(Resources.OnetimeValidSaml, "194.144.213.209"));
        }

        [TestMethod]
        public void ThrowsOnMissingDestinationSSN()
        {
            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Destination", "https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login");

#if NETFRAMEWORK
            svc = new IcelandAuthService();
            svc.Destination = "https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login";
#elif NETCOREAPP
            svc = new IcelandAuthService(configuration.Object, null, null);
#endif

            Assert.ThrowsException<InvalidOperationException>(() => svc.VerifySaml(Resources.OnetimeValidSaml, "194.144.213.209"));
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
            Assert.AreEqual("Íslykill", login.Authentication.First());
        }

#if NET5_0
        /// <summary>
        /// </summary>
        [TestMethod]
        public void VerifiesSignature()
        {
            var login = svc.VerifySaml(Resources.OnetimeValidSaml, null);

            Assert.IsTrue(login.SignatureOk);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void RecognizesInvalidSignature()
        {
            var login = svc.VerifySaml(Resources.SamlWithLongDuration_InvalidSignature, null);

            Assert.IsFalse(login.SignatureOk);
            Assert.IsTrue(login.TimeOk);
        }
#endif

        /// <summary>
        /// Accepts matching authentication
        /// </summary>
        [TestMethod]
        public void AcceptsMatchingAuthentication()
        {
            svc.Authentication = new string[] { "Íslykill" };

            var login = svc.VerifySaml(Resources.OnetimeValidSaml, null);

            Assert.IsTrue(login.AuthMethodOk);
        }

        /// <summary>
        /// Rejects on mismatched authentication
        /// </summary>
        [TestMethod]
        public void RejectsMismatchedAuthentication()
        {
            svc.Authentication = new string[] { "Rafræn skilríki" };

            var login = svc.VerifySaml(Resources.OnetimeValidSaml, null);

            Assert.IsFalse(login.AuthMethodOk);
        }
    }
}
