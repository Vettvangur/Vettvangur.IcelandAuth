using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
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

        /// <summary>
        /// Possible values include:
        /// "Rafræn skilríki"
        /// "Rafræn símaskilríki"
        /// "Íslykill"
        /// </summary>
        readonly protected string Authentication;

        /// <summary>
        /// Audience will most likely be the sites host name.
        /// F.x. Vettvangur demo is icelandauth.vettvangur.is
        /// </summary>
        readonly protected string Audience;

        /// <summary>
        /// SAML response url destination. F.x. https://icelandauth.vettvangur.is/umbraco/icelandauth/icelandauth/login
        /// </summary>
        readonly protected string Destination;

        /// <summary>
        /// The SSN used for contract with Ísland.is. F.x 5208130550
        /// </summary>
        readonly protected string DestinationSSN;

        /// <summary>
        /// Unique identifier for this contract with Ísland.is in Guid format
        /// </summary>
        readonly protected string AuthID;

        /// <summary>
        /// Take care when enabling this setting, sensitive data will be exposed.
        /// Never enable in production!
        /// </summary>
        readonly protected bool LogSamlResponse;

        readonly protected ILogger Logger;

#if NETFRAMEWORK
        /// <summary>
        /// Intended for .NET Framework
        /// 
        /// Keys are read in the following form from <see cref="IConfiguration"/>, appSettings:add:IcelandAuth.Audience
        /// </summary>
        /// <param name="logger"></param>
        ///// <param name="configuration">Optionally provide a ready built <see cref="IConfiguration"/></param>
        public IcelandAuthService(ILogger logger = null)
        {
            Logger = logger;

            Audience = ConfigurationManager.AppSettings["IcelandAuth.Audience"];
            Destination = ConfigurationManager.AppSettings["IcelandAuth.Destination"];
            DestinationSSN = ConfigurationManager.AppSettings["IcelandAuth.DestinationSSN"];
            //AuthID = ConfigurationManager.AppSettings["IcelandAuth.AuthID"];
            Authentication = ConfigurationManager.AppSettings["IcelandAuth.Authentication"];

            bool.TryParse(ConfigurationManager.AppSettings["IcelandAuth.LogSamlResponse"], out var logSamlResponse);
            LogSamlResponse = logSamlResponse;
        }
#endif

        public IcelandAuthService(
            ILogger logger,
            IConfiguration configuration
        )
        {
            Logger = logger;

            Audience = configuration["IcelandAuth:Audience"];
            Destination = configuration["IcelandAuth:Destination"];
            DestinationSSN = configuration["IcelandAuth:DestinationSSN"];
            //AuthID = configuration["IcelandAuth:AuthID"];
            Authentication = configuration["IcelandAuth:Authentication"];

            bool.TryParse(configuration["IcelandAuth:LogSamlResponse"], out var logSamlResponse);
            LogSamlResponse = logSamlResponse;
        }

        /// <summary>
        /// Originally based on C# sample provided by Ísland.is
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress">
        /// If provided with an IPv4 address, will verify ip address in SAML document matches
        /// </param>
        /// <returns></returns>
        public virtual SamlLogin VerifySaml(
            string token,
            string ipAddress = null)
        {
            Logger?.LogDebug("Verifying Saml");

            var login = new SamlLogin();

            if (string.IsNullOrEmpty(token))
            {
                Logger?.LogWarning("Null or empty token string");
                return login;
            }

            byte[] data;
            try
            {
                data = Convert.FromBase64String(token);
            }
            catch (FormatException ex)
            {
                Logger?.LogWarning(ex, "Invalid SAML response format");
                return login;
            }

            string decodedString;
            try
            {
                decodedString = Encoding.UTF8.GetString(data);
            }
            catch (ArgumentException ex)
            {
                Logger?.LogWarning(ex, "Invalid SAML response format");
                return login;
            }


            XmlDocument doc = new XmlDocument
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
                return login;
            }

            Logger?.LogDebug("Parsed SAML");

            SignedXml signedXml = new SignedXml(doc);

            // Retrieve signature
            XmlElement signedInfo = doc["Response"]?["Signature"];
            var certText = doc["Response"]?["Signature"]?["KeyInfo"]?["X509Data"]?["X509Certificate"]?.InnerText;
            if (certText == null)
            {
                throw new FormatException("Invalid SAML response format");
            }

            signedXml.LoadXml(signedInfo);
            byte[] certData = Encoding.UTF8.GetBytes(certText);

            try
            {
                using (X509Certificate2 cert = new X509Certificate2(certData))
                {
                    // Verify signature
                    login.SignatureOk = signedXml.CheckSignature(cert, false);
                    if (login.SignatureOk)
                        login.Message += "Signature OK. ";
                    else
                        login.Message += "Signature not OK. ";

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
                        login.Message += "Certificate is OK. ";
                    }
                    else
                    {
                        login.Message += "Certificate not OK. ";
                    }
                }
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                login.Message += "Certificate not OK. ";
            }
            finally
            {
                if (!login.SignatureOk || !login.CertOk)
                {
                    Logger?.LogWarning("Signature/Certificate error, possible forgery attempt");
                }
            }

            Logger?.LogDebug("Certificate verified");

            DateTime nowTime = DateTime.UtcNow;
            // Retrieve time from conditions and compare
            XmlElement conditions = doc["Response"]?["Assertion"]?["Conditions"];
            if (conditions?.Attributes["NotBefore"]?.Value == null
            || conditions?.Attributes["NotOnOrAfter"].Value == null)
            {
                throw new FormatException("Invalid SAML format");
            }

            DateTime fromTime =
                DateTime.Parse(conditions.Attributes["NotBefore"].Value);
            DateTime toTime =
                DateTime.Parse(conditions.Attributes["NotOnOrAfter"].Value);

            if (conditions["AudienceRestriction"]?["Audience"]?.InnerText
                .Equals(Audience, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                login.AudienceOk = true;
                login.Message += "Audience is OK. ";
            }
            else
            {
                login.Message += "Audience not OK. ";
                Logger?.LogWarning($"Audience mismatch, received {conditions["AudienceRestriction"]?["Audience"]?.InnerText}");
            }

            if (nowTime > fromTime && toTime > nowTime)
            {
                login.TimeOk = true;
                login.Message += "SAML time valid. ";
            }
            else if (nowTime < fromTime)
                login.Message += "From time has not passed yet. ";
            else if (nowTime > toTime)
                login.Message += "Too much time has passed. ";

            Logger?.LogDebug("Timestamp verified");

            // Verify ip address and authentication method if provided
            XmlNodeList attrList = doc["Response"]["Assertion"]["AttributeStatement"]?.ChildNodes;

            if (attrList?.Count > 0)
            {
                foreach (XmlNode attrNode in attrList)
                {
                    login.Attributes.Add(new IcelandAuthAttribute
                    {
                        Format = attrNode.Attributes["NameFormat"].Value,
                        Name = attrNode.Attributes["Name"].Value,
                        FriendlyName = attrNode.Attributes["FriendlyName"].Value,
                        Value = attrNode.FirstChild.InnerText
                    });
                }

                // IPAddress
                var ipAddressAttr = login.Attributes.FirstOrDefault(x => x.Name == "IPAddress");
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    login.IpOk = ipAddressAttr?.Value.Equals(ipAddress) == true;
                }
                else
                {
                    login.IpOk = true;
                }

                // Authentication method used, f.x. phone certificate.
                var authenticationAttr = login.Attributes.FirstOrDefault(x => x.Name == "Authentication");
                if (!string.IsNullOrEmpty(Authentication))
                {
                    login.AuthMethodOk = authenticationAttr?.Value == Authentication;

                    login.Authentication = authenticationAttr?.Value;
                }
                else
                {
                    login.AuthMethodOk = true;
                }

                login.UserSSN = login.Attributes.FirstOrDefault(x => x.Name == "UserSSN")?.Value;

                login.Name = login.Attributes.FirstOrDefault(x => x.Name == "Name")?.Value;

                login.OnbehalfRight = login.Attributes.FirstOrDefault(i => i.Name == "BehalfRight")?.Value;
                login.OnBehalfName = login.Attributes.FirstOrDefault(i => i.Name == "OnBehalfName")?.Value;
                login.OnbehalfSSN = login.Attributes.FirstOrDefault(i => i.Name == "OnBehalfUserSSN")?.Value;
                login.OnbehalfValue = login.Attributes.FirstOrDefault(i => i.Name == "BehalfValue")?.Value;

                var behalfValidityAttr = login.Attributes.FirstOrDefault(i => i.Name == "BehalfValidity");
                if (DateTime.TryParse(behalfValidityAttr?.Value, out var val))
                {
                    login.OnbehalfValidity = val;
                }

                if (login.IpOk)
                    login.Message += "Correct ip address. ";
                else
                    login.Message += "Incorrect ip address. ";

                if (login.AuthMethodOk)
                    login.Message += "Correct authentication method. ";
                else
                    login.Message += "Incorrect authentication method";

                Logger?.LogDebug("Attributes read");
            }
            else
            {
                login.Message += "No Attributes found";
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

            Logger?.LogDebug(login.Message);
            return login;
        }
    }
}
