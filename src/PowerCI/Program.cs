using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using PowerCI.Commands.Docker;
using PowerCI.Commands.Gitlab;
using PowerCI.Commands.Info;
using PowerCI.Commands.Jenkins;

namespace PowerCI
{
    [HelpOption(Inherited = true)]
    [Command(Description = "Power CI tools"),
     Subcommand(typeof(InfoCommand)), 
     Subcommand(typeof(DockerCommand)),
     Subcommand(typeof(JenkinsCommand)),
     Subcommand(typeof(GitlabCommand))]
    internal class Program
    {
        public static string Workspace { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".power-ci");

        static int Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(PhysicalConsole.Singleton);
            serviceCollection.AddSingleton<ICommandService, CommandService>();

            var services = serviceCollection.BuildServiceProvider();

            var app = new CommandLineApplication<Program>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            var console = (IConsole)services.GetService(typeof(IConsole));

            if (!Directory.Exists(Workspace))
            {
                Directory.CreateDirectory(Workspace);
            }

            if (!File.Exists(JenkinsCommand.ConfigPath))
            {
                File.Create(JenkinsCommand.ConfigPath);
            }

            try
            {
                return app.Execute(args);
            }
            catch (UnrecognizedCommandParsingException ex)
            {
                console.WriteLine(ex.Message);
                return -1;
            }
        }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("Please specify a command.");
            app.ShowHelp();
            return 1;
        }
    }
}