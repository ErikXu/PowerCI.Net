using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics;

namespace PowerCI
{
    internal interface ICommandService
    {
        int ExecuteCommand(string command);
    }

    internal class CommandService : ICommandService
    {
        private readonly IConsole _console;

        public CommandService(IConsole console)
        {
            _console = console;
        }

        // https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
        public int ExecuteCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode;
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                _console.WriteLine(outLine.Data);
            }
        }
    }
}
