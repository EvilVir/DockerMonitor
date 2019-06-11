using System;
using System.Threading;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        private readonly static ThreadLocal<Random> randomGenerator = new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId ^ Environment.TickCount));

        public static IDisposable BeginScope(this ILogger logger)
        {
            return logger.BeginScope($"{Thread.CurrentThread.ManagedThreadId}[{randomGenerator.Value.Next(1,9999).ToString().PadLeft(4, '0')}]");
        }

        public static EventId CreateEventId()
        {
            return new EventId(randomGenerator.Value.Next());
        }
    }
}
