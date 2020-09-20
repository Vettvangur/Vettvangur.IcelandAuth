using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Umbraco7
{
    /// <summary>
    /// Log4net compat for <see cref="ILogger"/>.
    /// Modified from https://dotnetthoughts.net/how-to-use-log4net-with-aspnetcore-for-logging/
    /// </summary>
    public class Log4NetLogger : ILogger<IcelandAuthService>
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="loggerRepository">The repository name.</param>
        /// <param name="name">The logger's name.</param>
        public Log4NetLogger(ILog logger)
            => this.log = logger;


        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
            => this.log.Logger.Name;

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return this.log.IsFatalEnabled;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return this.log.IsDebugEnabled;
                case LogLevel.Error:
                    return this.log.IsErrorEnabled;
                case LogLevel.Information:
                    return this.log.IsInfoEnabled;
                case LogLevel.Warning:
                    return this.log.IsWarnEnabled;
                case LogLevel.None:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

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
            if (!this.IsEnabled(logLevel))
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
                        this.log.Debug(message, exception);
                        break;

                    case LogLevel.Error:
                        this.log.Error(message, exception);
                        break;

                    case LogLevel.Information:
                        this.log.Info(message, exception);
                        break;

                    case LogLevel.Warning:
                        this.log.Warn(message, exception);
                        break;

                    case LogLevel.None:
                        // Just ignore the message. But this option shouldn't be reached.
                        break;

                    default:
                        this.log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        this.log.Info(message, exception);
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
