using Umbraco.Core;
using Umbraco.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Umbraco8
{
    /// <summary>
    /// The log4net logger class.
    /// Modified from https://dotnetthoughts.net/how-to-use-log4net-with-aspnetcore-for-logging/
    /// </summary>
    class UmbracoLogger : Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly Umbraco.Core.Logging.ILogger log;
        private readonly Type type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="loggerRepository">The repository name.</param>
        /// <param name="name">The logger's name.</param>
        public UmbracoLogger(Umbraco.Core.Logging.ILogger logger, Type t)
        {
            log = logger;
            type = t;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => type.Name;

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            => log.IsEnabled(type, (Umbraco.Core.Logging.LogLevel)logLevel);

        /// <summary>
        /// Logs an exception into the log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event Id.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            EnsureValidFormatter(formatter);

            string message = formatter(state, exception);
            bool shouldLogSomething = !string.IsNullOrEmpty(message) || exception != null;
            if (shouldLogSomething)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                        this.log.Debug(type, message + exception);
                        break;

                    case Microsoft.Extensions.Logging.LogLevel.Error:
                        this.log.Error(type, exception, message);
                        break;

                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        this.log.Info(type, message + exception);
                        break;

                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        this.log.Warn(type, exception, message);
                        break;

                    case Microsoft.Extensions.Logging.LogLevel.None:
                        // Just ignore the message. But this option shouldn't be reached.
                        break;

                    default:
                        this.log.Warn(type, "Encountered unknown log level {logLevel}, writing out as Info.", logLevel);
                        this.log.Info(type, message + exception);
                        break;
                }
            }
        }

        private static void EnsureValidFormatter<TState>(Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
        }

        IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
