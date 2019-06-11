using System;
using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Services.Commands
{
    public interface ICommandsExecutor
    {
        bool CanHandle(Type commandType);
        IEnumerable<string> Execute(IEnumerable<ICommand> commands);
        string Execute(ICommand command);
    }

    public interface ICommandsExecutor<T> : ICommandsExecutor
        where T : ICommand
    {
        IEnumerable<string> Execute(IEnumerable<T> commands);
        string Execute(T command);
    }
}
