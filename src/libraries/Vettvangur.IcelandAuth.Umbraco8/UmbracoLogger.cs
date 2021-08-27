using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vettvangur.IcelandAuth.Umbraco8
{
    /// <summary>
    /// Umbraco logger compat for <see cref="ILogger"/>.
    /// Modified from https://dotnetthoughts.net/how-to-use-log4net-with-aspnetcore-for-logging/
    /// </summary>
    public class UmbracoLogger : ILogger<IcelandAuthService>
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
        public bool IsEnabled(LogLevel logLevel)
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
            LogLevel logLevel,
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
                    case LogLevel.Debug:
                        this.log.Debug(type, message + exception);
                        break;

                    case LogLevel.Error:
                        this.log.Error(type, exception, message);
                        break;

                    case LogLevel.Information:
                        this.log.Info(type, message + exception);
                        break;

                    case LogLevel.Warning:
                        this.log.Warn(type, exception, message);
                        break;

                    case LogLevel.None:
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

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
