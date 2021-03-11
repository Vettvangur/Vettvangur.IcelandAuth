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

            composition.Register<IcelandAuthService>();
        }
    }
}
