using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vettvangur.IcelandAuth.Tests
{
    static class SimpleConfiguration
    {
        public static Mock<IConfiguration> Create() => new Mock<IConfiguration>();

        public static void SetSection(Mock<IConfiguration> config, string section, string value)
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns(value);

            config.Setup(x => x.GetSection(section)).Returns(configurationSection.Object);
        }
    }
}
