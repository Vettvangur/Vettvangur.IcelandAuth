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
            string nullString = null;
            Assert.ThrowsException<InvalidOperationException>(
                () => IcelandAuthService.CreateUrl(nullString));

            var svc = new IcelandAuthService(Mock.Of<IConfiguration>(), null);
            Assert.ThrowsException<InvalidOperationException>(() => svc.CreateLoginUrl());
        }

       [TestMethod]
        public void CreatesAuthUrl()
        {
#if NETFRAMEWORK
            var url = IcelandAuthService.CreateUrlWithId("test.icelandauth.vettvangur.is");
#else
            var url = IcelandAuthService.CreateUrl("test.icelandauth.vettvangur.is");
#endif

            Assert.AreEqual("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is", url);
        }

        [TestMethod]
        public void CreatesAuthConstrainedUrl()
        {
            var targetUri = new Uri($"https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=4");
            var targetQuery = System.Web.HttpUtility.ParseQueryString(targetUri.Query);

#if NETFRAMEWORK
            var url = IcelandAuthService.CreateUrlWithId(
                "test.icelandauth.vettvangur.is",
                null,
                new string[] { "Rafræn símaskilríki" }
            );
#else
            var url = IcelandAuthService.CreateUrl(
                "test.icelandauth.vettvangur.is",
                null,
                new string[] { "Rafræn símaskilríki" }
            );
#endif

            var createdUri = new Uri(url);
            var createdQuery = System.Web.HttpUtility.ParseQueryString(createdUri.Query);

            foreach (var kvp in targetQuery.AllKeys)
            {
                Assert.AreEqual(targetQuery[kvp], createdQuery[kvp]);
            }

            Assert.AreEqual(targetUri.Scheme + "://" + targetUri.Host + targetUri.AbsolutePath, createdUri.Scheme + "://" + createdUri.Host + createdUri.AbsolutePath);
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
            var targetUri = new Uri($"https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=3");
            var targetQuery = System.Web.HttpUtility.ParseQueryString(targetUri.Query);

            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:ID", "test.icelandauth.vettvangur.is");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Authentication", "Rafræn símaskilríki , Íslykill , Styrktur Íslykill");

            var svc = new IcelandAuthService(configuration.Object, null);

            var url = svc.CreateLoginUrl();
            var createdUri = new Uri(url);
            var createdQuery = System.Web.HttpUtility.ParseQueryString(createdUri.Query);

            foreach (var kvp in targetQuery.AllKeys)
            {
                Assert.AreEqual(targetQuery[kvp], createdQuery[kvp]);
            }

            Assert.AreEqual(targetUri.Scheme + "://" + targetUri.Host + targetUri.AbsolutePath, createdUri.Scheme + "://" + createdUri.Host + createdUri.AbsolutePath);
        }


        [TestMethod]
        public void CreatesAuthUrl_FromProperties()
        {
            var guid = Guid.NewGuid();

            var targetUri = new Uri($"https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=3&authid={guid}");
            var targetQuery = System.Web.HttpUtility.ParseQueryString(targetUri.Query);

            var svc = new IcelandAuthService(Mock.Of<IConfiguration>(), null);
            svc.IslandIsID = "test.icelandauth.vettvangur.is";
            svc.AuthID = guid;
            svc.Authentication = new string[] { "Rafræn símaskilríki", "Styrktur Íslykill" };

            var url = svc.CreateLoginUrl();
            var createdUri = new Uri(url);
            var createdQuery = System.Web.HttpUtility.ParseQueryString(createdUri.Query);

            foreach (var kvp in targetQuery.AllKeys)
            {
                Assert.AreEqual(targetQuery[kvp], createdQuery[kvp]);
            }

            Assert.AreEqual(targetUri.Scheme + "://" + targetUri.Host + targetUri.AbsolutePath, createdUri.Scheme + "://" + createdUri.Host + createdUri.AbsolutePath);
        }

        [TestMethod]
        public void ThrowsOnInvalidLang()
        {
            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:ID", "test.icelandauth.vettvangur.is");

            var svc = new IcelandAuthService(configuration.Object, null);

            Assert.ThrowsException<ArgumentException>(() => svc.CreateLoginUrl("bogus"));
        }

        [TestMethod]
        public void CreatesAuthUrl_WithLang()
        {
            var targetUri = new Uri("https://innskraning.island.is/?id=test.icelandauth.vettvangur.is&qaa=3");
            var targetQuery = System.Web.HttpUtility.ParseQueryString(targetUri.Query);

            var configuration = SimpleConfiguration.Create();
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:ID", "test.icelandauth.vettvangur.is");
            SimpleConfiguration.SetSection(configuration, "IcelandAuth:Authentication", "Rafræn símaskilríki , Íslykill , Styrktur Íslykill");

            var svc = new IcelandAuthService(configuration.Object, null);

            var url = svc.CreateLoginUrl();
            var createdUri = new Uri(url);
            var createdQuery = System.Web.HttpUtility.ParseQueryString(createdUri.Query);

            foreach (var kvp in targetQuery.AllKeys)
            {
                Assert.AreEqual(targetQuery[kvp], createdQuery[kvp]);
            }

            Assert.AreEqual(targetUri.Scheme + "://" + targetUri.Host + targetUri.AbsolutePath, createdUri.Scheme + "://" + createdUri.Host + createdUri.AbsolutePath);
        }
    }
}
