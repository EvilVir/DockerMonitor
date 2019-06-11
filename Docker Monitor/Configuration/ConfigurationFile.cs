using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StrangeFog.Docker.Monitor.Converters;
using StrangeFog.Docker.Monitor.Services.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace StrangeFog.Docker.Monitor.Configuration
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ConfigurationFile
    {
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, LoggingTargetConfiguration> Logging { get; set; }

        [JsonProperty(Required = Required.Default)]
        public DockerEngineConfiguration Docker { get; set; } = new DockerEngineConfiguration();

        [JsonProperty(Required = Required.Always)]
        public ContainersGroupsConfiguration Groups { get; set; } = new ContainersGroupsConfiguration();

        [JsonProperty(Required = Required.Default)]
        public PluginsConfiguration Plugins { get; set; } = new PluginsConfiguration();

        public static ConfigurationFile Load(string path)
        {
            if (File.Exists(path))
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Converters.Add(new ContainersGroupConfigurationActionsConverter());

                return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(path), serializerSettings);
            }
            else
            {
                throw new FileNotFoundException($"Configuration file not found at path \"{path}\"", path);
            }
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LoggingTargetConfiguration
    {
        [JsonProperty(Required = Required.Always)]
        public LogLevel[] Levels { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string Target { get; set; }
    }

    public class ContainersGroupsConfiguration : Dictionary<string, ContainersGroupConfiguration>
    {
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class DockerEngineConfiguration
    {
        [JsonProperty(Required = Required.Default)]
        public string Uri { get; set; }

        [JsonProperty(Required = Required.Default)]
        public DockerEngineBasicAuthConfiguration BasicAuthorization { get; set; }

        [JsonProperty(Required = Required.Default)]
        public DockerEngineX509AuthConfiguration CertificateAuthorization { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class DockerEngineBasicAuthConfiguration
    {
        [JsonProperty(Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string Password { get; set; }

        [JsonProperty(Required = Required.Default)]
        public bool UseTls { get; set; } = false;
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class DockerEngineX509AuthConfiguration
    {
        [JsonProperty(Required = Required.Always)]
        public string File { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string Password { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ContainersGroupConfiguration
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public int Interval { get; set; } = 60;

        [JsonProperty(Required = Required.Always)]
        public List<string> Containers { get; set; } = new List<string>();

        [JsonProperty(Required = Required.Always)]
        public Dictionary<ContainersGroupState, List<ICommand>> Actions { get; set; } = new Dictionary<ContainersGroupState, List<ICommand>>();
    }

    public class PluginsConfiguration : Dictionary<string, JObject>
    {
        internal object GetPluginConfiguration(string configurationNodeName, Type configurationType)
        {
            return this.ContainsKey(configurationNodeName) ? this[configurationNodeName].ToObject(configurationType) : null;
        }
    }
}
