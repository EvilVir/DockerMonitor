using System;
using System.Collections.Generic;

namespace StrangeFog.Docker.Monitor.Services.Commands
{
    public abstract class CommandsExecutorBase<T> : ICommandsExecutor<T>
        where T : ICommand
    {
        public abstract IEnumerable<string> Execute(IEnumerable<T> commands);
        public abstract string Execute(T command);

        public bool CanHandle(Type commandType)
        {
            return commandType.Equals(typeof(T));
        }

        public IEnumerable<string> Execute(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                yield return Execute(command);
            }
        }

        public string Execute(ICommand command)
        {
            return Execute((T)command);
        }
    }
}
