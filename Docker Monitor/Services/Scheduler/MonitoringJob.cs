using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Services.Docker;
using System;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor.Services.Scheduler
{
    [DisallowConcurrentExecution]
    public class MonitoringJob : IJob
    {
        public const string JOB_DATA_GROUP_NAME = "GroupName";

        public async Task Execute(IJobExecutionContext context)
        {
            var groupName = context.JobDetail.JobDataMap.GetString(JOB_DATA_GROUP_NAME);
            await Execute(groupName);
        }

        public async Task Execute(string groupName)
        {
            var logger = ServicesContainer.Provider.GetRequiredService<ILoggerFactory>().CreateLogger($"{nameof(MonitoringJob)} :: {groupName}");

            using (logger.BeginScope())
            {
                try
                {
                    logger.LogInformation(LogEventId.MONITORING_JOB_START, "Executing job {JobName}", groupName);

                    var factory = ServicesContainer.Provider.GetRequiredService<ContainersMonitoringServiceFactory>();
                    var config = ServicesContainer.Provider.GetRequiredService<ConfigurationFile>().Groups[groupName];
                    var monitor = factory.Create(groupName, config);
                    await monitor.Execute();
                }
                catch (Exception e)
                {
                    logger.LogError(LogEventId.MONITORING_JOB_ERROR, "Exception of type {ExceptionType} while executing job {JobName}: {Message}", e.GetType(), groupName, e.Message);

                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace(LogEventId.MONITORING_JOB_ERROR_DETAILS, e.StackTrace);
                    }

                    throw;
                }

                logger.LogInformation(LogEventId.MONITORING_JOB_END, "Job {JobName} completed", groupName);
            }
        }
    }
}
