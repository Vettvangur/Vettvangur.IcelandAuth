using Microsoft.Extensions.Logging;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vettvangur.IcelandAuth.Owin
{
    class LoggingBridge : ILogger<IcelandAuthService>
    {
        private readonly Microsoft.Owin.Logging.ILogger _logger;

        public LoggingBridge(Microsoft.Owin.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
        public bool IsEnabled(LogLevel logLevel)
            => _logger.WriteCore(DefaultGetLogLevel(logLevel), 0, null, null, null);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);
            bool shouldLogSomething = !string.IsNullOrEmpty(message) || exception != null;
            if (shouldLogSomething)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        _logger.WriteVerbose(message + exception);
                        break;

                    case LogLevel.Error:
                        _logger.WriteError(message, exception);
                        break;

                    case LogLevel.Information:
                        _logger.WriteInformation(message + exception);
                        break;

                    case LogLevel.Warning:
                        _logger.WriteWarning(message, exception);
                        break;

                    case LogLevel.None:
                        // Just ignore the message. But this option shouldn't be reached.
                        break;

                    default:
                        _logger.WriteWarning($"Encountered unknown log level {logLevel}, writing out as Info.");
                        _logger.WriteInformation(message + exception);
                        break;
                }
            }
        }

        /// <summary>
        /// This is the standard translation
        /// </summary>
        /// <param name="traceEventType"></param>
        /// <returns></returns>
        static TraceEventType DefaultGetLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return TraceEventType.Critical;
                case LogLevel.Error:
                    return TraceEventType.Error;
                case LogLevel.Warning:
                    return TraceEventType.Warning;
                case LogLevel.Information:
                    return TraceEventType.Information;
                case LogLevel.Trace:
                    return TraceEventType.Verbose;
                case LogLevel.Debug:
                    return TraceEventType.Verbose;
                default:
                    throw new ArgumentOutOfRangeException("traceEventType");
            }
        }
    }
}
