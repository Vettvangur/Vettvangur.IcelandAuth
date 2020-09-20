#if NETFRAMEWORK
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth
{
    /// <summary>
    /// https://benfoster.io/blog/net-core-configuration-legacy-projects/
    /// </summary>
    public class LegacyConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public static IConfigurationRoot Create() =>
            new ConfigurationBuilder()
                .Add(new LegacyConfigurationProvider())
                .Build();

        public override void Load()
        {
            foreach (var settingKey in ConfigurationManager.AppSettings.AllKeys)
            {
                Data.Add(settingKey, ConfigurationManager.AppSettings[settingKey]);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }
    }
}
#endif
