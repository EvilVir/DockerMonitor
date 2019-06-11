using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrangeFog.Docker.Monitor.Services.Commands
{
    public class CommandsExecutorFactory
    {
        protected readonly ILogger logger;
        protected readonly IServiceProvider serviceProvider;

        public CommandsExecutorFactory(ILogger<CommandsExecutorFactory> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            logger.LogInformation(LogEventId.COMMAND_EXECUTOR_COUNT, "There are {Count} executors registered", serviceProvider.GetServices<ICommandsExecutor>().Count());
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
            if (command != null)
            {
                var cmdType = command.GetType();
                var executor = serviceProvider.GetServices<ICommandsExecutor>()
                                                .Where(x => x.CanHandle(cmdType))
                                                .First();

                return executor.Execute(command);
            }

            return null;
        }
    }
}
