using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;



var rootCommand = new RootCommand("Example application that greets the user.");
var nameOption = new Option<string>("--name", "The name of the person to greet.");
var greetingOption = new Option<string>("--greeting", () => "Hello", "The greeting to use.");

rootCommand.AddOption(nameOption);
rootCommand.AddOption(greetingOption);

rootCommand.Handler = CommandHandler.Create<string, string>((name, greeting) =>
{
    Console.WriteLine($"{greeting}, {name}!");
});

await rootCommand.InvokeAsync(args);




CreateHostBuilder(null).Build().Run();

static IHostBuilder CreateHostBuilder(string[]? args) =>
    Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        var env = hostContext.HostingEnvironment;
        config.AddUserSecrets<Program>();
        if (env.IsDevelopment())
        {

        }
    })
        .ConfigureServices((hostContext, services) =>
        {

            services.ConfigureServices(hostContext.Configuration);

        });