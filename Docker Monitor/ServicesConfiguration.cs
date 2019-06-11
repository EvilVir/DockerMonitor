using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.EventLog;
using Quartz;
using Quartz.Impl;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Events;
using StrangeFog.Docker.Monitor.Extensions;
using StrangeFog.Docker.Monitor.Plugins;
using StrangeFog.Docker.Monitor.Services;
using StrangeFog.Docker.Monitor.Services.Commands;
using StrangeFog.Docker.Monitor.Services.Commands.Shell;
using StrangeFog.Docker.Monitor.Services.Docker;
using StrangeFog.Docker.Monitor.Services.Scheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace StrangeFog.Docker.Monitor
{
    public static class ServicesConfiguration
    {
        public static void Configure(IServiceCollection services, CliOptions options, ConfigurationFile configuration)
        {
            ConfigureLogging(services, options, configuration);

            services.AddSingleton(x => options)
                    .AddSingleton(x => configuration)
                    .AddSingleton(x => configuration.Groups)
                    .AddSingleton(x => new CancellationTokenSource())
                    .AddSingleton<GlobalCancellationToken>()
                    .AddSingleton<DockerClientConfigurationFactory>()
                    .AddSingleton(x => x.GetRequiredService<DockerClientConfigurationFactory>().Create(configuration.Docker))
                    .AddSingleton<DockerClientFactory>()
                    .AddSingleton<ContainersMonitoringServiceFactory>()
                    .AddSingleton<ScheduledMonitoringService>()
                    .AddSingleton<CommandsExecutorFactory>()
                    .AddSingleton<ISchedulerFactory, StdSchedulerFactory>()
                    .AddSingleton<EventsBus>()
                    .AddSingleton(x => x.GetRequiredService<ISchedulerFactory>().GetScheduler().Result);

            services.AddTransient<ICommandsExecutor, ShellCommandsExecutor>();

            ConfigurePlugins(services, configuration);
        }

        private static void ConfigureLogging(IServiceCollection services, CliOptions options, ConfigurationFile configuration)
        {
            services.AddLogging(x =>
            {
                x.ClearProviders();

                var filters = new Dictionary<string, LogLevel[]>();

                if (configuration.Logging?.TryGetValue("EventLog") is LoggingTargetConfiguration eventLogConfig)
                {
                    var target = eventLogConfig.Target?.Split('/');

                    x.AddEventLog(new EventLogSettings()
                    {
                        MachineName = target?.TryGetAt(0, null, str => !string.IsNullOrEmpty(str) && str != "."),
                        LogName = target?.TryGetAt(1, null, str => !string.IsNullOrEmpty(str) && str != "."),
                        SourceName = target?.TryGetAt(2, null, str => !string.IsNullOrEmpty(str) && str != "."),
                    });

                    filters.Add(typeof(EventLogLoggerProvider).FullName, eventLogConfig.Levels);
                }

                if (configuration.Logging?.TryGetValue("Console") is LoggingTargetConfiguration consoleLogConfig)
                {
                    x.AddConsole((consoleLoggerOptions) => consoleLoggerOptions.IncludeScopes = true);
                    filters.Add(typeof(ConsoleLoggerProvider).FullName, consoleLogConfig.Levels);
                }

                x.AddFilter((t, c, l) =>
                {
                    return filters.TryGetValue(t)?.Contains(l) == true;
                });
            });
        }

        private static void ConfigurePlugins(IServiceCollection services, ConfigurationFile configuration)
        {
            var pluginsDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var file in new DirectoryInfo(pluginsDir).GetFiles($"DockerMonitor.Plugins.*.dll"))
            {
                var assembly = Assembly.LoadFile(file.FullName);
                ConfigurePlugins(assembly, services, configuration);
            }
        }

        private static void ConfigurePlugins(Assembly assembly, IServiceCollection services, ConfigurationFile configuration)
        {
            var registerMethodName = nameof(IPluginEntrypoint<object>.Register);
            var definitions = assembly.GetExportedTypes()
                                .Where(x => !x.IsAbstract && !x.IsInterface && x.IsPublic)
                                .Select(x => new { Type = x, EntryPointInterface = x.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPluginEntrypoint<>)).FirstOrDefault() })
                                .Where(x => x.EntryPointInterface != null)
                                .Select(x =>
                                {
                                    var configurationType = x.EntryPointInterface.GetGenericArguments().First();
                                    return new
                                    {
                                        x.Type,
                                        ConfigurationType = configurationType,
                                        ConfigurationNodeName = x.Type.Name,
                                        RegistrationMethod = x.Type.GetMethod(registerMethodName, new Type[] { configurationType })
                                    };
                                });

            foreach (var definition in definitions)
            {
                var pluginConfiguration = configuration.Plugins.GetPluginConfiguration(definition.ConfigurationNodeName, definition.ConfigurationType);

                if (pluginConfiguration != null)
                {
                    var entrypoint = Activator.CreateInstance(definition.Type);
                    var output = (IEnumerable<PluginEventHandlerRegistration>)definition.RegistrationMethod.Invoke(entrypoint, new object[] { pluginConfiguration });

                    foreach (var reg in output)
                    {
                        foreach (var iface in reg.HandlerInterfaces)
                        {
                            if (reg.IsSingleton)
                            {
                                services.AddSingleton(iface, reg.ServiceType);
                            }
                            else
                            {
                                services.AddScoped(iface, reg.ServiceType);
                            }
                        }
                    }
                }
            }
        }
    }
}
