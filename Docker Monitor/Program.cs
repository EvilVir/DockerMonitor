using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Services;
using StrangeFog.Docker.Monitor.Services.Scheduler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (Parser.TryParse(args, out CliOptions options))
            {
                try
                {
                    var configuration = ConfigurationFile.Load(options.ConfigurationFile);
                    ServicesContainer.Configure(x => ServicesConfiguration.Configure(x, options, configuration));

                    if (string.IsNullOrEmpty(options.CheckNow))
                    {
                        using (var cancellationSource = ServicesContainer.Provider.GetRequiredService<GlobalCancellationToken>().Source)
                        {
                            var monitor = ServicesContainer.Provider.GetRequiredService<ScheduledMonitoringService>();
                            monitor.Start();

                            Console.WriteLine($"Press [CTRL+C] to exit...");

                            var exitEvent = new ManualResetEventSlim(false);

                            Console.CancelKeyPress += (s, e) =>
                            {
                                e.Cancel = true;
                                exitEvent.Set();
                            };

                            exitEvent.Wait(cancellationSource.Token);

                            if (!cancellationSource.IsCancellationRequested)
                            {
                                cancellationSource.Cancel();
                            }

                            monitor.Stop();
                        }
                    }
                    else
                    {
                        var job = new MonitoringJob();
                        await job.Execute(options.CheckNow);
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    Parser.DisplayHelp<CliOptions>();

                    Console.WriteLine();
                    Console.WriteLine($"[Error: {e.Message}]");

                    return e.GetType().GetHashCode();
                }
            }
            else
            {
                return 1;
            }
        }
    }
}
