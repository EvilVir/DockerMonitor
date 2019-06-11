using CommandLine.Attributes;

namespace StrangeFog.Docker.Monitor.Configuration
{
    public class CliOptions
    {
        [OptionalArgument("DockerMonitor.json", "config", "Path to configuration file")]
        public string ConfigurationFile { get; set; }

        [OptionalArgument(null, "checknow", "Will just check group, which name you need to pass here, and then exit without staying in background")]
        public string CheckNow { get; set; }
    }
}
