using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
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
        /// Possible values include:
        /// "Rafræn skilríki"
        /// "Rafræn símaskilríki"
        /// </summary>
        readonly protected string Authentication;

        /// <summary>
        /// Audience will most likely be the sites host name.
        /// F.x. Mín Líðan audience is www.minlidan.is
        /// </summary>
        readonly protected string Audience;

        /// <summary>
        /// If true we will never log the saml response
        /// False and the saml response is logged at log level Debug
        /// </summary>
        readonly protected bool NeverTraceXmlDocument;

        readonly protected ILogger Logger;

#if NET461
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
            Authentication = ConfigurationManager.AppSettings["IcelandAuth.Authentication"];
            bool.TryParse(ConfigurationManager.AppSettings["IcelandAuth.NeverTraceXmlDocument"], out var shouldNeverTrace);
            NeverTraceXmlDocument = shouldNeverTrace;
        }
#endif

        public IcelandAuthService(
            ILogger logger,
            IConfiguration configuration
        )
        {
            Logger = logger;

            Audience = configuration["IcelandAuth:Audience"];
            Authentication = configuration["IcelandAuth:Authentication"];
            bool.TryParse(configuration["IcelandAuth:NeverTraceXmlDocument"], out var shouldNeverTrace);
            NeverTraceXmlDocument = shouldNeverTrace;
        }

        /// <summary>
        /// Originally based on C# sample provided by Ísland.is
        /// </summary>
        /// <param name="samlString"></param>
        /// <param name="ipAddress">
        /// If provided with an IPv4 address, will verify ip address in SAML document matches
        /// </param>
        /// <returns></returns>
        public virtual SamlLogin VerifySaml(
            string samlString,
            string ipAddress = null)
        {
            var login = new SamlLogin();
            string message = string.Empty;

            byte[] data = Convert.FromBase64String(samlString);
            string decodedString = Encoding.UTF8.GetString(data);

            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            doc.LoadXml(decodedString);

            SignedXml signedXml = new SignedXml(doc);

            // Retrieve signature
            XmlElement signedInfo = doc["Response"]["Signature"];
            var certText = doc["Response"]["Signature"]["KeyInfo"]["X509Data"]["X509Certificate"].InnerText;
            signedXml.LoadXml(signedInfo);
            byte[] certData = Encoding.UTF8.GetBytes(certText);

            bool signatureOk;
            bool certOk = false;
            using (X509Certificate2 cert = new X509Certificate2(certData))
            {
                // Verify signature
                signatureOk = signedXml.CheckSignature(cert, false);
                if (signatureOk)
                    message += "Signature OK. ";
                else
                    message += "Signature not OK. ";

                if (cert.Issuer.StartsWith("CN=Traustur bunadur")
                && cert.Subject.Contains("SERIALNUMBER=6503760649"))
                {
                    certOk = true;
                    message += "Certificate is OK. ";
                }
                else
                    message += "Certificate not OK. ";
            }

            DateTime nowTime = DateTime.UtcNow;
            // Retrieve time from conditions and compare
            XmlElement conditions = doc["Response"]["Assertion"]["Conditions"];
            DateTime fromTime =
                DateTime.Parse(conditions.Attributes["NotBefore"].Value);
            DateTime toTime =
                DateTime.Parse(conditions.Attributes["NotOnOrAfter"].Value);

            bool audienceOk = false;
            if (conditions["AudienceRestriction"]["Audience"].InnerText
                .Equals(Audience, StringComparison.InvariantCultureIgnoreCase))
            {
                audienceOk = true;
                message += "Audience is OK. ";
            }
            else
            {
                message += "Audience not OK. ";
            }

            bool timeOk = false;

            if (nowTime > fromTime && toTime > nowTime)
            {
                timeOk = true;
                message += "SAML time valid. ";
            }
            else if (nowTime < fromTime)
                message += "From time has not passed yet. ";
            else if (nowTime > toTime)
                message += "Too much time has passed. ";

            // Read SSN and name from document
            bool ssnOk = false;
            bool nameOk = false;

            // Verify ip address and authentication method if provided
            bool ipOk = true;
            bool authMethodOk = true;

            XmlNodeList attrList = doc["Response"]["Assertion"]["AttributeStatement"].ChildNodes;

            if (attrList.Count > 0)
            {
                foreach (XmlNode attrNode in attrList)
                {
                    XmlAttributeCollection attrCol = attrNode.Attributes;

                    // IPAddress
                    if (attrCol["Name"].Value.Equals("IPAddress"))
                    {
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            ipOk = attrNode.FirstChild.InnerText.Equals(ipAddress);
                        }
                    }

                    // Authentication method used, f.x. phone certificate.
                    else if (attrCol["Name"].Value.Equals("Authentication"))
                    {
                        if (!string.IsNullOrEmpty(Authentication))
                        {
                            authMethodOk = attrNode.FirstChild.InnerText == Authentication;
                        }

                        login.AuthenticationUsed = attrNode.FirstChild.InnerText;
                    }

                    // UserSSN
                    else if (attrCol["Name"].Value.Equals("UserSSN"))
                    {
                        if (!string.IsNullOrEmpty(attrNode.FirstChild.InnerText) &&
                            attrNode.FirstChild.InnerText.Length == 10)
                        {
                            ssnOk = long.TryParse(attrNode.FirstChild.InnerText, out var unused);
                            login.UserSSN = attrNode.FirstChild.InnerText;
                        }
                    }

                    // Name
                    else if (attrCol["Name"].Value.Equals("Name"))
                    {
                        if (!string.IsNullOrEmpty(attrNode.FirstChild.InnerText))
                        {
                            login.Name = attrNode.FirstChild.InnerText;
                            nameOk = true;
                        }
                    }
                }

                if (ipOk)
                    message += "Correct ip address. ";
                else
                    message += "Incorrect ip address. ";

                if (ssnOk)
                    message += "Correct SSN. ";
                else
                    message += "Incorrect SSN";

                if (authMethodOk)
                    message += "Correct authentication method. ";
                else
                    message += "Incorrect authentication method";
            }
            else
                message += "No Attributes found";

            if (!NeverTraceXmlDocument)
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();

                    Logger?.LogDebug("XMLDoc: \r\n" + stringWriter.GetStringBuilder().ToString());
                }
            }

            if (signatureOk && certOk && timeOk && ssnOk && nameOk && ipOk && authMethodOk && audienceOk)
            {
                return login;
            }
            else
            {
                Logger?.LogError(message);
                return null;
            }
        }
    }
}
