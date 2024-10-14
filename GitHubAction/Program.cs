using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddGitHubActionServices())
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);

parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("GitHubAction.Program")
            .LogError(
                string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => Action(options, host));

await host.RunAsync();

static async Task Action(ActionInputs inputs, IHost host)
{
    using CancellationTokenSource tokenSource = new();

    Console.CancelKeyPress += delegate
    {
        tokenSource.Cancel();
    };
    
    var adGuardHomeService = Get<IAdGuardHomeService>(host);

    try
    {
        await adGuardHomeService.SyncRewrites(inputs.Url, inputs.UserName, inputs.UserPassword, inputs.Path, true, tokenSource.Token);
    }
    catch (Exception e)
    {
        var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(Action));
        logger.LogError(e.Message);
        
        Environment.Exit(2);
    }

    Environment.Exit(0);
}