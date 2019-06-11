using System;
using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Plugins
{
    public interface IPluginEntrypoint<TConfiguration>
        where TConfiguration : class, new()
    {
        IEnumerable<PluginEventHandlerRegistration> Register(TConfiguration configuration);
    }
}
