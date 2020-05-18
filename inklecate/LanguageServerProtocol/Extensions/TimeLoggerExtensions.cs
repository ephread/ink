using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol.Extensions
{
    // Reusing the elegant solution of Omisharp's Language Server package
    // for logging time.
    //
    // Originally licensed under the terms ot the MIT License.
    //
    // The idea is simple, the methods here will keep track of the current
    // time when they're first called and then log the elapsed time
    // once they're disposed, at the end of the `using` statement block.
    public static class TimeLoggerExtensions
    {
        class Disposable: IDisposable
        {
            private readonly IDisposable _disposable;
            private readonly Action<long> _action;
            private readonly Stopwatch _sw;

            public Disposable(IDisposable disposable, Action<long> action)
            {
                _disposable = disposable;
                _action = action;
                _sw = new Stopwatch();
                _sw.Start();
            }

            public void Dispose()
            {
                _sw.Stop();
                _action(_sw.ElapsedMilliseconds);
                _disposable.Dispose();
            }
        }

        /// <summary>
        /// Times the trace.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>IDisposable.</returns>
        public static IDisposable TimeTrace(this ILogger logger, string message, params object[] args)
        {
            var scope = logger.BeginScope(new { });
            logger.LogTrace($"Starting: {message}", args);
            return new Disposable(scope, elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogTrace($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            });
        }

        /// <summary>
        /// Times the debug.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>IDisposable.</returns>
        public static IDisposable TimeDebug(this ILogger logger, string message, params object[] args)
        {
            var scope = logger.BeginScope(new { });
            logger.LogDebug($"Starting: {message}", args);
            return new Disposable(scope, elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogDebug($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            });
        }

        /// <summary>
        /// Times the information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>IDisposable.</returns>
        public static IDisposable TimeInformation(this ILogger logger, string message, params object[] args)
        {
            var scope = logger.BeginScope(new { });
            logger.LogInformation($"Starting: {message}", args);
            return new Disposable(scope, elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogInformation($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            });
        }
    }
}