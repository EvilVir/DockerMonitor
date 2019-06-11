using StrangeFog.Docker.Monitor.Events;
using System;
using System.Linq;

namespace StrangeFog.Docker.Monitor.Plugins
{
    public class PluginEventHandlerRegistration
    {
        public bool IsSingleton { get; protected set; }
        public Type ServiceType { get; protected set; }
        public Type[] HandlerInterfaces { get; protected set; }

        private PluginEventHandlerRegistration() { }

        public static PluginEventHandlerRegistration Create<THandler>(bool isSingleton = true)
            where THandler : IEventHandler
        {
            return new PluginEventHandlerRegistration()
            {
                IsSingleton = isSingleton,
                ServiceType = typeof(THandler),
                HandlerInterfaces = typeof(THandler).GetInterfaces()
                                             .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                                             .ToArray()
            };
        }
    }
}
