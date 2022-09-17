using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using PowerCI.Commands.Info;

namespace PowerCI
{
    [HelpOption(Inherited = true)]
    [Command(Description = "Power CI tools"),
     Subcommand(typeof(InfoCommand))]
    internal class Program
    {
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