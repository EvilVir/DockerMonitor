using System;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor.Events
{
    public interface IEventHandler
    {
    }

    public interface IEventHandler<T> : IEventHandler
        where T : EventArgs
    {
        Task HandleAsync(T @event);
    }
}
