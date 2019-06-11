using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace StrangeFog.Docker.Monitor.Services.Commands.Shell
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ShellCommand : ICommand
    {
        [Flags]
        public enum ErrorDetectingMode
        {
            ExitCode = 1,
            StdErr = 2,
            Ignore = 4
        }

        [JsonProperty(Required = Required.Default)]
        public string WorkingDirectory { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Command { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string Arguments { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ErrorDetectingMode ErrorDetecting { get; set; } = ErrorDetectingMode.ExitCode;

        public bool Equals(ICommand other)
        {
            return other != null && other is ShellCommand otherSC &&
                    otherSC.WorkingDirectory == this.WorkingDirectory &&
                    otherSC.Command == this.Command &&
                    otherSC.Arguments == this.Arguments &&
                    otherSC.ErrorDetecting == this.ErrorDetecting;
        }

        public override bool Equals(object obj)
        {
            return obj is ICommand otherCommand && Equals(otherCommand);
        }

        public override int GetHashCode()
        {
            return (WorkingDirectory?.GetHashCode() ?? 0) ^
                   (Command?.GetHashCode() ?? 0) ^
                   (Arguments?.GetHashCode() ?? 0) ^
                   ((int)ErrorDetecting).GetHashCode();
        }
    }
}
