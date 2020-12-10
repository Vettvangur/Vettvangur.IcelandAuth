#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Xml;

namespace Vettvangur.IcelandAuth
{
    /// <summary>
    /// Process SAML messages received from Ísland.is authentication service
    /// </summary>
    public partial class IcelandAuthService
    {
        /// <summary>
        /// SSN of SAML Signer(Þjóðskrá)
        /// </summary>
        protected const string SignerSSN = "6503760649";
        /// <summary>
        /// Name of issuing CA
        /// </summary>
        protected const string IssuerName = "Traustur bunadur";
        /// <summary>
        /// SSN of issuing CA (Auðkenni)
        /// </summary>
        protected const string IssuerSSN = "5210002790";

        private IEnumerable<string> _authentication;
        /// <summary>
        /// Rafræn skilríki – Digital certificate authentication. <br />
        /// Rafræn símaskilríki - Digital certificate authentication using a phone. <br />
        /// Rafræn starfsmannaskilríki – Employee digital certificate authentication. <br />
        /// Íslykill – Authentication using Íslykill. <br />
        /// Styrktur Íslykill – 2FA using Íslykill, 2FA delivered via phone or email. <br />
        /// Styrkt rafræn skilríki – Digital certificate authentication with 2FA via phone/email. <br />
        /// Styrkt rafræn starfsmannaskilríki – Employee digital certificate authentication with 2FA via phone/email. <br />
        /// </summary>
        public virtual IEnumerable<string> Authentication
        {
            get => _authentication;
            set => _authentication = value?.Select(x => x.Trim(' '));
        }

        /// <summary>
        /// island.is generates the audience attribute by reading the host
        /// portion of the destination url.
        /// 
        /// </summary>
        private string _audience;

        private string _destination;
        /// <summary>
        /// SAML response url destination. F.x. https://icelandauth.vettvangur.is/umbraco/surface/icelandauth/login
        /// </summary>
        public virtual string Destination
        {
            get => _destination;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var uri = new Uri(value);
                    _audience = uri.Host;
                    _destination = value;
                }
            }
        }

        /// <summary>
        /// The SSN used in the contract with Ísland.is.
        /// </summary>
        public virtual string DestinationSSN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Guid? AuthID { get; set; }

        /// <summary>
        /// Check if the users IP matches the one seen at authentication.
        /// Not always a reliable check, breaks when site is hosted on internal network
        /// Default true
        /// </summary>
        public virtual bool VerifyIPAddress { get; set; }

        /// <summary>
        /// Take care when enabling this setting, sensitive data will be logged.
        /// Never enable in production!
        /// </summary>
        public virtual bool LogSamlResponse { get; set; }

        readonly protected ILogger Logger;

#if NETFRAMEWORK
        /// <summary>
        /// .NET Framework constructor, reads key/values from <see cref="ConfigurationManager.AppSettings"/>.
        /// </summary>
        /// <param name="logger"></param>
        public IcelandAuthService(ILogger<IcelandAuthService> logger = null)
            : this(LegacyConfigurationProvider.Create(), logger)
        {
        }
#endif

#if NETCOREAPP
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// .NET Core constructor
        /// </summary>
        public IcelandAuthService(
            IConfiguration configuration,
            ILogger<IcelandAuthService> logger,
            IHttpContextAccessor httpContextAccessor
        ) : this(configuration, logger)
        {
            _httpContextAccessor = httpContextAccessor;
        }
#endif

#if NETCOREAPP || NETFRAMEWORK
        internal IcelandAuthService(
#else
        public IcelandAuthService(
#endif
            IConfiguration configuration,
            ILogger<IcelandAuthService> logger
        )
        {
            Logger = logger;

            if (configuration != null)
            {
                Destination = configuration["IcelandAuth:Destination"];
                DestinationSSN = configuration["IcelandAuth:DestinationSSN"];
                IslandIsID = configuration["IcelandAuth:ID"];
                AuthID = string.IsNullOrEmpty(configuration["IcelandAuth:AuthID"])
                    ? null
                    // Throw on non-guid
                    : (Guid?)Guid.Parse(configuration["IcelandAuth:AuthID"]);
                Authentication = string.IsNullOrEmpty(configuration["IcelandAuth:Authentication"])
                    ? null
                    : configuration["IcelandAuth:Authentication"].Split(',');
                VerifyIPAddress = bool.TryParse(configuration["IcelandAuth:VerifyIPAddress"], out var verifyIpAddress)
                    ? verifyIpAddress
                    : true;

                bool.TryParse(configuration["IcelandAuth:LogSamlResponse"], out var logSamlResponse);
                LogSamlResponse = logSamlResponse;
            }
        }

#if NETFRAMEWORK
        public virtual SamlLogin VerifySaml()
        {
            var httpCtx = HttpContext.Current;
            return VerifySaml(httpCtx.Request.Form["token"], httpCtx.Request.UserHostAddress);
        }
#elif NETCOREAPP
        public virtual SamlLogin VerifySaml()
        {
            if (_httpContextAccessor == null)
            {
                throw new InvalidOperationException("IHttpContextAccessor is required if no token is provided");
            }

            var req = _httpContextAccessor.HttpContext.Request;
            if (req.Method == "POST" && req.Form.ContainsKey("token"))
            {
                return VerifySaml(
                    req.Form["token"],
                    _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
            }

            return null;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress">
        /// Verify ip address in SAML document matches, 
        /// can be disabled with IcelandAuth.VerifyIPAddress appSetting
        /// </param>
        /// <returns></returns>
        public virtual SamlLogin VerifySaml(
            string token,
            string ipAddress)
        {
            // We verify required params here to allow consumers to override web.config
            // by setting the public properties on the service
            if (string.IsNullOrEmpty(Destination))
            {
                throw new InvalidOperationException(
                    $"Missing {nameof(Destination)} url, either configure in app settings or set the service property."
                );
            }
            if (string.IsNullOrEmpty(DestinationSSN))
            {
                throw new InvalidOperationException(
                    $"Missing {nameof(DestinationSSN)}, either configure in app settings or set the service property."
                );
            }

            Logger?.LogDebug("Verifying Saml");

            var login = new SamlLogin();

            if (string.IsNullOrEmpty(token))
            {
                Logger?.LogWarning("Null or empty token string");
                return login;
            }

            var doc = ParseToken(token);
            if (doc == null)
            {
                return login;
            }

            var signedXml = new SignedXml(doc);

            // Retrieve signature
            XmlElement signedInfo = doc["Response"]?["Signature"];
            var certText = doc["Response"]?["Signature"]?["KeyInfo"]?["X509Data"]?["X509Certificate"]?.InnerText;
            if (certText == null)
            {
                Logger?.LogWarning("Invalid SAML response format");
                return login;
            }

            signedXml.LoadXml(signedInfo);
            byte[] certData = Encoding.UTF8.GetBytes(certText);

            VerifySignature(login, signedXml, certData);

            // Retrieve time from conditions and compare
            XmlElement conditions = doc["Response"]?["Assertion"]?["Conditions"];
            if (conditions?.Attributes["NotBefore"]?.Value == null
            || conditions?.Attributes["NotOnOrAfter"].Value == null)
            {
                Logger?.LogWarning("Invalid SAML response format");
                return login;
            }

            DateTime nowTime = DateTime.UtcNow;

            DateTime fromTime =
                DateTime.Parse(conditions.Attributes["NotBefore"].Value);
            DateTime toTime =
                DateTime.Parse(conditions.Attributes["NotOnOrAfter"].Value);

            if (nowTime > fromTime && toTime > nowTime)
            {
                login.TimeOk = true;
            }
            else if (nowTime < fromTime)
            {
                Logger?.LogWarning("From time has not passed yet.");
            }
            else if (nowTime > toTime)
            {
                Logger?.LogInformation("Too much time has passed.");
            }

            Logger?.LogDebug("Timestamp verified");

            if (conditions["AudienceRestriction"]?["Audience"]?.InnerText
                .Equals(_audience, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                login.AudienceOk = true;
            }
            else
            {
                Logger?.LogWarning($"Audience mismatch, received {conditions["AudienceRestriction"]?["Audience"]?.InnerText}. Ensure IcelandAuth:Destination is correctly configured.");
            }

            var destination = doc.DocumentElement.Attributes["Destination"].Value;

            if (Destination.Equals(destination, StringComparison.InvariantCultureIgnoreCase))
            {
                login.DestinationOk = true;
            }
            else
            {
                Logger?.LogWarning("Destination mismatch, received " + destination);
            }

            // Verify ip address and authentication method if provided
            XmlNodeList attrList = doc["Response"]["Assertion"]["AttributeStatement"]?.ChildNodes;

            if (attrList?.Count > 0)
            {
                foreach (XmlNode attrNode in attrList)
                {
                    if (attrNode.Attributes != null)
                    {
                        login.Attributes.Add(new IcelandAuthAttribute
                        {
                            Format = attrNode.Attributes["NameFormat"].Value,
                            Name = attrNode.Attributes["Name"].Value,
                            FriendlyName = attrNode.Attributes["FriendlyName"].Value,
                            Value = attrNode.FirstChild.InnerText
                        });
                    }
                }

                // IPAddress
                if (VerifyIPAddress && !string.IsNullOrEmpty(ipAddress))
                {
                    var ipAddressAttr = login.Attributes.First(x => x.Name == "IPAddress");
                    login.IpOk = ipAddressAttr.Value.Equals(ipAddress);

                    if (!login.IpOk)
                    {
                        Logger?.LogWarning($"IP Address mismatch, received {ipAddress} but read {ipAddressAttr.Value} from Saml");
                    }
                }
                else
                {
                    login.IpOk = true;
                }

                // Authentication method used, f.x. phone certificate.
                var authenticationResp = login.Attributes.First(x => x.Name == "Authentication").Value;
                if (Authentication?.Any() == true)
                {
                    login.AuthMethodOk = Authentication.Contains(authenticationResp);

                    if (!login.AuthMethodOk)
                    {
                        Logger?.LogInformation($"Authentication method not OK, received {authenticationResp}");
                    }
                }
                else
                {
                    login.AuthMethodOk = true;
                }

                var authIdResp = login.Attributes.FirstOrDefault(x => x.Name == "AuthID")?.Value;
                if (AuthID != null)
                {
                    if (Guid.TryParse(authIdResp, out var authId) && AuthID == authId)
                    {
                        login.AuthIdOk = true;
                    }
                    else
                    {
                        Logger?.LogWarning("AuthId mismatch, received " + authIdResp);
                    }
                }
                else
                {
                    login.AuthIdOk = true;
                }

                var destSsnResp = login.Attributes.First(x => x.Name == "DestinationSSN").Value;
                if (!string.IsNullOrEmpty(DestinationSSN))
                {
                    if (DestinationSSN == destSsnResp)
                    {
                        login.DestinationSsnOk = true;
                    }
                    else
                    {
                        Logger?.LogWarning("DestinationSSN mismatch, received " + destSsnResp);
                    }
                }
                else
                {
                    login.DestinationSsnOk = true;
                }

                Logger?.LogDebug("Attributes read");
            }
            else
            {
                Logger?.LogWarning("No Attributes found");
            }

            if (LogSamlResponse)
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();

                    Logger?.LogDebug("SAML Response: \r\n" + stringWriter.GetStringBuilder().ToString());
                }
            }

            if (login.Valid)
            {
                Logger?.LogInformation("island.is valid");
            }
            else
            {
                Logger?.LogInformation("island.is invalid");
            }

            return login;
        }

        protected virtual XmlDocument ParseToken(string token)
        {
            byte[] data;
            try
            {
                data = Convert.FromBase64String(token);
            }
            catch (FormatException ex)
            {
                Logger?.LogWarning(ex, "Invalid SAML response format");
                return null;
            }

            string decodedString;
            try
            {
                decodedString = Encoding.UTF8.GetString(data);
            }
            catch (ArgumentException ex)
            {
                Logger?.LogWarning(ex, "Invalid SAML response format");
                return null;
            }


            var doc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            try
            {
                doc.LoadXml(decodedString);
            }
            catch (XmlException ex)
            {
                Logger?.LogWarning(ex, "Invalid SAML response format");
                return null;
            }

            Logger?.LogDebug("Parsed SAML");


            return doc;
        }

        protected virtual void VerifySignature(SamlLogin login, SignedXml signedXml, byte[] certData)
        {
            try
            {
                using (X509Certificate2 cert = new X509Certificate2(certData))
                {
#if NET5
                    // Verify signature only
                    login.SignatureOk = signedXml.CheckSignature(cert, true);

                    // When using CustomRootTrust with just Audkennisrot trusted there is no need to verify issuer manually

                    using var certChain = new X509Chain();
                    certChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    certChain.ChainPolicy.CustomTrustStore.Add(AudkenniCertificateChain.Audkennisrot);
                    certChain.ChainPolicy.ExtraStore.Add(AudkenniCertificateChain.TraustAudkenni);
                    certChain.ChainPolicy.ExtraStore.Add(AudkenniCertificateChain.TrausturBunadur);

                    if (certChain.Build(cert))
                    {
                        login.CertOk = true;
                        Logger?.LogDebug("Certificate verified");
                    }
#else
                    // Verify signature
                    login.SignatureOk = signedXml.CheckSignature(cert, false);

                    var issuerComponents = ADUtils.GetDNComponents(cert.Issuer);
                    var subjectComponents = ADUtils.GetDNComponents(cert.Subject);
                    var issuerNameComponent = issuerComponents.FirstOrDefault(x => x.Name == "CN");
                    var issuerSerialComponent = issuerComponents.FirstOrDefault(x => x.Name == "SERIALNUMBER");
                    var subjectSerialComponent = subjectComponents.FirstOrDefault(x => x.Name == "SERIALNUMBER");

                    // default(<struct>) is never null, will also never match our constant values.
                    if (issuerNameComponent.Value == IssuerName
                    && issuerSerialComponent.Value == IssuerSSN
                    && subjectSerialComponent.Value == SignerSSN)
                    {
                        login.CertOk = true;
                        Logger?.LogDebug("Certificate verified");
                    }
#endif
                }
            }
            // Invalid certificate, continue on for further logging but validation has failed at this point.
            catch (System.Security.Cryptography.CryptographicException) { }
            finally
            {
                if (!login.SignatureOk)
                {
                    Logger?.LogWarning("Signature error, possible forgery attempt");
                }
                if (!login.CertOk)
                {
                    Logger?.LogWarning("Certificate error, possible forgery attempt");
                }
            }
        }
    }
}
