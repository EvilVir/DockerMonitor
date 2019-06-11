using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StrangeFog.Docker.Monitor.Services.Commands.Shell
{
    public class ShellCommandsExecutor : CommandsExecutorBase<ShellCommand>
    {
        protected readonly ILogger logger;

        public ShellCommandsExecutor(ILogger<ShellCommandsExecutor> logger)
        {
            this.logger = logger;
        }

        public override IEnumerable<string> Execute(IEnumerable<ShellCommand> commands)
        {
            foreach (var command in commands)
            {
                yield return Execute(command);
            }
        }

        public override string Execute(ShellCommand command)
        {
            using (logger.BeginScope())
            {
                logger.LogInformation(LogEventId.COMMAND_EXECUTOR_START, "Executing command");

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace(LogEventId.COMMAND_EXECUTOR_START_DETAILS, "Working directory: {WorkingDirectory}; Command: {Command}; Arguments: {Arguments}", command.WorkingDirectory, command.Command, command.Arguments);
                }

                try
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        Arguments = command.Arguments,
                        FileName = command.Command,
                        WorkingDirectory = command.WorkingDirectory,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    var process = new Process()
                    {
                        StartInfo = startInfo
                    };

                    process.Start();
                    process.WaitForExit();

                    var exitCode = process.ExitCode;
                    var stdOut = process.StandardOutput.ReadToEnd().Trim();
                    var stdErr = process.StandardError.ReadToEnd().Trim();

                    if (!command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.Ignore))
                    {
                        var checkCodeAndErr = command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.ExitCode) && command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.StdErr);
                        var checkOnlyCode = command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.ExitCode) && !command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.StdErr);
                        var checkOnlyErr = !command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.ExitCode) && command.ErrorDetecting.HasFlag(ShellCommand.ErrorDetectingMode.StdErr);

                        var isError = (checkCodeAndErr && exitCode > 0 && !string.IsNullOrEmpty(stdErr)) ||
                                      (checkOnlyCode && exitCode > 0) ||
                                      (checkOnlyErr && !string.IsNullOrEmpty(stdErr));


                        var logLevel = isError ? LogLevel.Error : LogLevel.Trace;

                        if (!string.IsNullOrEmpty(stdOut))
                        {
                            logger.Log(logLevel, LogEventId.COMMAND_EXECUTOR_STDOUT, "StdOut: {StandardOutput}", stdOut);
                        }

                        if (!string.IsNullOrEmpty(stdErr))
                        {
                            logger.Log(logLevel, LogEventId.COMMAND_EXECUTOR_STDERR, "StdErr: {StandardError}", stdErr);
                        }
                    }

                    logger.LogInformation(LogEventId.COMMAND_EXECUTOR_STOP, "Command executed, exit code: {ExitCode}", exitCode);

                    var output = (!string.IsNullOrEmpty(stdOut) ? stdOut : "") + (!string.IsNullOrEmpty(stdErr) ? stdErr : "");
                    return !string.IsNullOrEmpty(output) ? output : null;
                }
                catch (Exception e)
                {
                    logger.LogError(LogEventId.COMMAND_EXECUTOR_EXCEPTION, "Exception {Type} while executing shell command: {Message}", e.GetType(), e.Message);
                    logger.LogTrace(LogEventId.COMMAND_EXECUTOR_EXCEPTION_DETAILS, e.StackTrace);

                    return $"Exception while executing shell command: {e.GetType()} - {e.Message}";
                }
            }
        }
    }
}
