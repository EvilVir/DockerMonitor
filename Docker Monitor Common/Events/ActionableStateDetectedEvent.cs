using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Services.Commands;
using System;
using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Events
{
    public class ActionableStateDetectedEvent : EventArgs
    {
        public class ActionDetails
        {
            public ICommand Command { get; set; }
            public string Output { get; set; }
        }

        public string Group { get; set; }
        public Dictionary<string, List<string>> ContainerStates { get; set; } = new Dictionary<string, List<string>>();
        public List<ContainersGroupState> GroupStates { get; set; }
        public Dictionary<ContainersGroupState, List<ActionDetails>> ActionsOutput { get; set; } = new Dictionary<ContainersGroupState, List<ActionDetails>>();
    }
}
