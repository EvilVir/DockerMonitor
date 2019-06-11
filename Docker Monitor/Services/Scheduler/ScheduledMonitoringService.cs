using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Quartz;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Services.Docker;

namespace StrangeFog.Docker.Monitor.Services.Scheduler
{
    public class ScheduledMonitoringService
    {
        protected readonly ILogger logger;
        protected readonly DockerClientConfiguration dockerClientConfiguration;
        protected readonly ContainersGroupsConfiguration groupsConfiguration;
        protected readonly ContainersMonitoringServiceFactory monitoringServiceFactory;
        protected readonly GlobalCancellationToken globalCancellationToken;
        protected readonly IScheduler scheduler;

        protected readonly bool isStarted = false;

        public ScheduledMonitoringService(ILogger<ScheduledMonitoringService> logger, 
                                DockerClientConfiguration dockerClientConfiguration, 
                                ContainersGroupsConfiguration groupsConfiguration, 
                                ContainersMonitoringServiceFactory monitoringServiceFactory,
                                GlobalCancellationToken globalCancellationToken,
                                IScheduler scheduler)
        {
            this.logger = logger;
            this.dockerClientConfiguration = dockerClientConfiguration;
            this.groupsConfiguration = groupsConfiguration;
            this.monitoringServiceFactory = monitoringServiceFactory;
            this.globalCancellationToken = globalCancellationToken;
            this.scheduler = scheduler;
        }

        public void Start()
        {
            Stop();

            using (logger.BeginScope())
            {
                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_START_BEGIN, "Starting monitor service");

                foreach (var group in groupsConfiguration.Keys)
                {
                    var interval = groupsConfiguration[group].Interval;

                    var job = JobBuilder.Create<MonitoringJob>()
                                .WithIdentity($"JOB_{group}")
                                .UsingJobData(MonitoringJob.JOB_DATA_GROUP_NAME, group)
                                .Build();

                    var trigger = TriggerBuilder.Create()
                                    .WithIdentity($"TRIGGER_{group}")
                                    .StartNow()
                                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(interval)
                                                              .RepeatForever()
                                                              .WithMisfireHandlingInstructionIgnoreMisfires())
                                    .Build();

                    var offset = scheduler.ScheduleJob(job, trigger).Result;

                    logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_START_DETAILS, "Created MonitoringJob {Group} with interval trigger {Interval}s", group, interval);
                }

                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_START_SCHEDULER_STARTING, "Starting scheduler");
                scheduler.Start(globalCancellationToken.Token);

                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_START_DONE, "Monitor service started");
            }
        }

        public void Pause()
        {
            using (logger.BeginScope())
            {
                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_PAUSE_BEGIN, "Pausing monitor service");
                scheduler.PauseAll(globalCancellationToken.Token);
                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_PAUSE_DONE, "Monitor service paused");
            }
        }

        public void Resume()
        {
            using (logger.BeginScope())
            {
                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_RESUME_BEGIN, "Resuming monitor service");
                scheduler.ResumeAll(globalCancellationToken.Token);
                logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_RESUME_DONE, "Monitor service resumed");
            }
        }

        public void Stop()
        {
            using (logger.BeginScope())
            {
                if (isStarted)
                {
                    logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_STOP_BEGIN, "Stoping monitor service");
                    scheduler.Shutdown(globalCancellationToken.Token).Wait();
                    logger.LogInformation(LogEventId.SCHEDULED_MONITORING_SERVICE_STOP_DONE, "Monitor service stopped");
                }
            }
        }
    }
}
