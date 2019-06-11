using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Plugins.Mailer
{
    public class Mailer : IPluginEntrypoint<Configuration>
    {
        public static Configuration Config { get; protected set; }

        public IEnumerable<PluginEventHandlerRegistration> Register(Configuration configuration)
        {
            Mailer.Config = configuration;

            yield return PluginEventHandlerRegistration.Create<ActionableStateDetectedEventHandler>();
        }
    }
}
