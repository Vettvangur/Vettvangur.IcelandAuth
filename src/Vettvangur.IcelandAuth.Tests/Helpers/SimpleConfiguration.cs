using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vettvangur.IcelandAuth.Tests
{
    static class SimpleConfiguration
    {
        public static Mock<IConfiguration> Create()
        {
            var conf = new Mock<IConfiguration>();
            return conf;
        }

        public static void SetSection(Mock<IConfiguration> config, string key, string value)
        {
            config.SetupGet(x => x[key]).Returns(value);
        }
    }
}
