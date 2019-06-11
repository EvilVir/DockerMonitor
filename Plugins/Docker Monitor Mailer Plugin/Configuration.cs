using StrangeFog.Docker.Monitor.Configuration;
using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Plugins.Mailer
{
    public class Configuration
    {
        public class ServerConfiguration
        {
            public string Host { get; set; }
            public int? Port { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public string From { get; set; }
            public bool SSL { get; set; }
        }

        public class Action
        {
            public List<string> Recipients { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
        }

        public ServerConfiguration Server { get; set; }
        public Dictionary<ContainersGroupState, Action> Actions { get; set; }
    }
}
