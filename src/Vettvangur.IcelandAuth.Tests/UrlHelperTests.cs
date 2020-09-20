using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vettvangur.IcelandAuth.Tests
{
    [TestClass]
    public class UrlHelperTests
    {
        [TestMethod]
        public void ThrowsOnMissingIslandId()
        {
            Assert.ThrowsException<InvalidOperationException>(() => IcelandAuthService.CreateUrl(null));

            var svc = new IcelandAuthService(Mock.Of<IConfiguration>(), null);
            Assert.ThrowsException<InvalidOperationException>(svc.CreateLoginUrl);
        }

        [TestMethod]
        public void CreatesAuthUrl()
        {
            var url = IcelandAuthService.CreateUrl("test.icelandauth.vettvangur.is");

            Assert.AreEqual("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is", url);
        }

        [TestMethod]
        public void CreatesAuthConstrainedUrl()
        {
            var url = IcelandAuthService.CreateUrl(
                "test.icelandauth.vettvangur.is",
                new string[] { "Rafræn símaskilríki" }
            );

            Assert.AreEqual("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=4", url);
        }

        [TestMethod]
        public void CreatesUnconstrainedAuthUrl()
        {
            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:ID", "test.icelandauth.vettvangur.is");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Authentication", "Rafræn símaskilríki , Íslykill ");

            var svc = new IcelandAuthService(configuration.Object, null);

            var url = svc.CreateLoginUrl();

            Assert.AreEqual("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is", url);
        }

        [TestMethod]
        public void Creates2FAConstrainedAuthUrl()
        {
            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:ID", "test.icelandauth.vettvangur.is");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Authentication", "Rafræn símaskilríki , Íslykill , Styrktur Íslykill");

            var svc = new IcelandAuthService(configuration.Object, null);

            var url = svc.CreateLoginUrl();

            Assert.AreEqual("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=3", url);
        }


        [TestMethod]
        public void CreatesAuthUrl_FromProperties()
        {
            var guid = Guid.NewGuid();

            var svc = new IcelandAuthService(Mock.Of<IConfiguration>(), null);
            svc.IslandIsID = "test.icelandauth.vettvangur.is";
            svc.AuthID = guid;
            svc.Authentication = new string[] { "Rafræn símaskilríki", "Styrktur Íslykill" };

            var url = svc.CreateLoginUrl();

            Assert.AreEqual($"https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=3&authid={guid}", url);
        }
    }
}
