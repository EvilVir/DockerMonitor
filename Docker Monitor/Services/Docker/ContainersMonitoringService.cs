using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using StrangeFog.Docker.Monitor.Configuration;
using StrangeFog.Docker.Monitor.Events;
using StrangeFog.Docker.Monitor.Services.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrangeFog.Docker.Monitor.Services.Docker
{
    public class ContainersMonitoringService
    {
        protected static readonly string[] DOWN_STATES = new[] { "exited" };
        protected static readonly string[] UP_STATES = new[] { "running" };

        protected readonly ILogger logger;
        protected readonly string groupName;
        protected readonly ContainersGroupConfiguration configuration;
        protected readonly DockerClientFactory dockerClientFactory;
        protected readonly ContainersListParameters listParameters;
        protected readonly GlobalCancellationToken globalCancellation;
        protected readonly CommandsExecutorFactory commandsExecutorFactory;
        protected readonly EventsBus eventsBus;

        public ContainersMonitoringService(ILogger logger,
                                            string groupName,
                                            ContainersGroupConfiguration configuration, 
                                            DockerClientFactory dockerClientFactory, 
                                            GlobalCancellationToken globalCancellation, 
                                            CommandsExecutorFactory commandsExecutorFactory,
                                            EventsBus eventsBus)
        {
            this.logger = logger;
            this.groupName = groupName;
            this.configuration = configuration;
            this.dockerClientFactory = dockerClientFactory;
            this.globalCancellation = globalCancellation;
            this.commandsExecutorFactory = commandsExecutorFactory;
            this.eventsBus = eventsBus;

            this.listParameters = new ContainersListParameters()
            {
                All = true,
                Filters = ComposeFilter(configuration)
            };

        }

        protected Dictionary<string, IDictionary<string, bool>> ComposeFilter(ContainersGroupConfiguration configuration)
        {
            var output = new Dictionary<string, IDictionary<string, bool>>();

            var namesFilter = new Dictionary<string, bool>();

            foreach (var name in configuration.Containers)
            {
                namesFilter.Add(name, true);
            }

            output.Add("name", namesFilter);
            logger.LogInformation(LogEventId.MONITORING_SERVICE_FILTER_CREATED, "Created filter for group {Group}: {Filter}", groupName, output);

            return output;
        }

        public async Task Execute()
        {
            using (logger.BeginScope())
            {
                try
                {
                    logger.LogInformation(LogEventId.MONITORING_SERVICE_CHECK_START, "Starting check routine of {Group}", groupName);

                    using (var client = dockerClientFactory.Create())
                    {
                        var list = await client.Containers.ListContainersAsync(listParameters, globalCancellation.Token);
                        logger.LogDebug(LogEventId.MONITORING_SERVICE_GOT_INFO, "Got information about {Count} containers in {Group}", list.Count, groupName);

                        var states = GroupStates(list);
                        var groupStates = CalculateGroupStates(states, configuration.Containers.Count).ToList();
                        var output = ExecuteActions(groupStates, configuration.Actions);

                        if (output.Any())
                        {
                            // Don't await, let it run in background, we don't care about the output
                            var publishTask = eventsBus.PublishAsync(new ActionableStateDetectedEvent()
                            {
                                Group = groupName,
                                GroupStates = groupStates,
                                ContainerStates = states,
                                ActionsOutput = output.ToDictionary(k => k.Key, v => v.Value.Select(x => new ActionableStateDetectedEvent.ActionDetails() { Command = x.Command, Output = x.Output }).ToList())
                            });
                        }
                    }

                    logger.LogInformation(LogEventId.MONITORING_SERVICE_CHECK_COMPLETED, "Check routine of {Group} completed", groupName);
                }
                catch (Exception e)
                {
                    logger.LogError(LogEventId.MONITORING_SERVICE_CHECK_ERROR, "Error occured: {Message}", e.Message);
                    throw;
                }
            }
        }

        protected Dictionary<ContainersGroupState, List<(ICommand Command, string Output)>> ExecuteActions(IEnumerable<ContainersGroupState> states, Dictionary<ContainersGroupState, List<ICommand>> actions)
        {
            using (logger.BeginScope())
            {
                var output = new Dictionary<ContainersGroupState, List<(ICommand Command, string Output)>>();

                logger.LogDebug(LogEventId.MONITORING_SERVICE_ACTIONS_DETAILS, "Group is in following states: {States}", string.Join(", ", states));

                foreach (var state in states)
                {
                    foreach (var kvp in actions)
                    {
                        if (kvp.Key.HasFlag(state))
                        {
                            logger.LogInformation(LogEventId.MONITORING_SERVICE_ACTIONS_MATCH_DETAILS, "Found matching actions group {ActionsGroupFlags} with {ActionsCount} actions to execute for state {State}", kvp.Key, kvp.Value.Count, state);

                            if (!output.ContainsKey(state))
                            {
                                output.Add(state, new List<(ICommand Command, string Output)>());
                            }

                            foreach (var command in kvp.Value)
                            {
                                var actionOutput = commandsExecutorFactory.Execute(command);
                                output[state].Add((Command: command, Output : actionOutput));
                            }
                        }
                    }
                }

                return output;
            }
        }

        protected IEnumerable<ContainersGroupState> CalculateGroupStates(Dictionary<string, List<string>> states, int totalContainersCount)
        {
            var totalUp = CountContainersInState(states, UP_STATES);
            var totalDown = CountContainersInState(states, DOWN_STATES);
            var totalMissing = totalContainersCount - totalUp - totalDown;

            logger.LogDebug(LogEventId.MONITORING_SERVICE_ACTIONS_CALCULATOR, "Containers stats: {UpCount} up, {DownCount} down, {MissingCount} missing", totalUp, totalDown, totalMissing);

            if (totalUp > 0)
            {
                yield return ContainersGroupState.AnyUp;
            }

            if (totalUp == totalContainersCount)
            {
                yield return ContainersGroupState.AllUp;
            }

            if (totalDown > 0)
            {
                yield return ContainersGroupState.AnyDown;
            }

            if (totalDown == totalContainersCount)
            {
                yield return ContainersGroupState.AllDown;
            }

            if (totalMissing > 0)
            {
                yield return ContainersGroupState.AnyMissing;
            }

            if (totalMissing == totalContainersCount)
            {
                yield return ContainersGroupState.AllMissing;
            }
        }

        protected int CountContainersInState(Dictionary<string, List<string>> states, params string[] state)
        {
            return states.Where(x => state.Contains(x.Key)).SelectMany(x => x.Value).Count();
        }

        protected Dictionary<string, List<string>> GroupStates(IEnumerable<ContainerListResponse> list)
        {
            var output = new Dictionary<string, List<string>>();

            foreach (var container in list)
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogDebug(LogEventId.MONITORING_SERVICE_ACTIONS_CALCULATOR_STATES, "Container {ContainerId} ({ContainerName}) is in state {State}", container.ID, string.Join(", ", container.Names), container.State);
                }

                if (!output.ContainsKey(container.State))
                {
                    output.Add(container.State, new List<string>());
                }

                output[container.State].Add(container.ID);
            }

            return output;
        }
    }
}
