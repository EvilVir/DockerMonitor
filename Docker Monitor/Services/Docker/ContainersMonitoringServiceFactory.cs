using Microsoft.Extensions.Logging;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Events;
using StrangeFog.Docker.Monitor.Services.Commands;
using System.Collections.Concurrent;

namespace StrangeFog.Docker.Monitor.Services.Docker
{
    public class ContainersMonitoringServiceFactory
    {
        protected readonly ILogger logger;
        protected readonly ILoggerFactory loggerFactory;
        protected readonly DockerClientFactory dockerClientFactory;
        protected readonly GlobalCancellationToken globalCancellation;
        protected readonly CommandsExecutorFactory commandsExecutorFactory;
        protected readonly EventsBus eventsBus;
        protected readonly ConcurrentDictionary<string, ContainersMonitoringService> servicesCache = new ConcurrentDictionary<string, ContainersMonitoringService>();

        public ContainersMonitoringServiceFactory(ILogger<ContainersMonitoringServiceFactory> logger, ILoggerFactory loggerFactory, DockerClientFactory dockerClientFactory, GlobalCancellationToken globalCancellation, CommandsExecutorFactory commandsExecutorFactory, EventsBus eventsBus)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.dockerClientFactory = dockerClientFactory;
            this.globalCancellation = globalCancellation;
            this.commandsExecutorFactory = commandsExecutorFactory;
            this.eventsBus = eventsBus;
        }

        public ContainersMonitoringService Create(string name, ContainersGroupConfiguration configuration)
        {
            return servicesCache.GetOrAdd(name, (x) =>
            {
                logger.LogInformation(LogEventId.MONITORING_SERVICE_FACTORY_CREATED, "Creating containers monitoring service for group {GroupName}", name);
                return new ContainersMonitoringService(CreateLogger(name), name, configuration, dockerClientFactory, globalCancellation, commandsExecutorFactory, eventsBus);
            });
        }

        protected ILogger CreateLogger(string groupName)
        {
            return loggerFactory.CreateLogger($"{nameof(ContainersMonitoringService)}[{groupName}]");
        }
    }
}
