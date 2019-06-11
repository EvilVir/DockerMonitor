using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Services.Commands;
using StrangeFog.Docker.Monitor.Services.Commands.Shell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrangeFog.Docker.Monitor.Converters
{
    public class ContainersGroupConfigurationActionsConverter : JsonConverter
    {
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        protected class CommandStub
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public string Type { get; set; } = nameof(ShellCommand);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Dictionary<ContainersGroupState, List<ICommand>>).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);
            return json.ToObject<Dictionary<ContainersGroupState, List<JObject>>>()
                        .ToDictionary(k => k.Key, v => ResolveCommandsDefinition(v.Value));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected List<ICommand> ResolveCommandsDefinition(List<JObject> json)
        {
            return json.Select(x => ResolveCommandDefinition(x)).ToList();
        }

        protected ICommand ResolveCommandDefinition(JObject json)
        {
            var stub = json.ToObject<CommandStub>();
            var targetType = FindCommandType(stub.Type);
            return (ICommand)json.ToObject(targetType);
        }

        protected Type FindCommandType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.Name == typeName && x.GetInterfaces().Contains(typeof(ICommand)))
                        .FirstOrDefault();
        }
    }
}
