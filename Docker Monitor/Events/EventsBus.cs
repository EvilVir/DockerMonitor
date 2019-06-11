using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor.Events
{
    public class EventsBus
    {
        protected readonly ILogger logger;
        protected readonly IServiceProvider serviceProvider;

        public EventsBus(IServiceProvider serviceProvider, ILogger<EventsBus> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task PublishAsync<T>(T @event)
            where T : EventArgs
        {
            using (var scope = serviceProvider.CreateScope())
            {
                using (logger.BeginScope())
                {
                    var eventType = typeof(T);

                    logger.LogDebug(LogEventId.EVENTS_BUS_PUBLISH_START, "Publishing event {EventType} to the bus", eventType);

                    var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

                    foreach (var handler in handlers)
                    {
                        logger.LogTrace(LogEventId.EVENTS_BUS_HANDLER_START, "Starting event handling by {Method}", handler);
                        await handler.HandleAsync(@event);
                        logger.LogTrace(LogEventId.EVENTS_BUS_HANDLER_END, "Completed event handling by {Method}", handler);
                    }

                    logger.LogDebug(LogEventId.EVENTS_BUS_PUBLISH_END, "Completed publishing event {EventType} to the bus", eventType);
                }
            }
        }
    }
}
