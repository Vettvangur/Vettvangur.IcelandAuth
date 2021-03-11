using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Vettvangur.IcelandAuth.Umbraco8.App_Start
{
    /// <summary>
    /// Use [ComposeAfter] to override
    /// </summary>
    public class IcelandAuthStartup : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<ILogger<IcelandAuthService>>(
              fac => new UmbracoLogger(
                fac.GetInstance<Umbraco.Core.Logging.ILogger>(),
                typeof(IcelandAuthService)));

            // We would love to have this lifetime Transient
            // Umbraco however doesn't provide an easy way to hook into the startup lifecycle until
            // after the first request to the LightInject container is made
            // https://github.com/seesharper/LightInject/issues/133
            // After that first request, registrations to the container are not overridable
            // unless we use the Umbraco custom RegisterUnique which keeps it's own Dictionary
            // of registrations.
            composition.RegisterUnique<IcelandAuthService>();
        }
    }
}
