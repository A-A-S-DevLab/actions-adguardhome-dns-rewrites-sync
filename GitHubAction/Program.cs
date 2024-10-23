using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddGitHubActionServices())
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

var parser = Default.ParseArguments(() => new ActionInputs(), args);

parser.WithNotParsed(errors =>
{
    Get<ILoggerFactory>(host)
        .CreateLogger("GitHubAction.Program")
        .LogError(
            string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

    Environment.Exit(2);
});

await parser.WithParsedAsync(inputs =>
{
    try
    {
        return Action(inputs, host);
    }
    catch (Exception e)
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("GitHubAction.Program")
            .LogError(e.Message);
        
        Environment.Exit(2);
    }

    return null;
});

Environment.Exit(0);

static async Task Action(ActionInputs inputs, IHost host)
{
    using CancellationTokenSource tokenSource = new();

    Console.CancelKeyPress += delegate
    {
        tokenSource.Cancel();
    };
    
    var adGuardHomeService = Get<IAdGuardHomeService>(host);

    await adGuardHomeService.SyncRewrites(inputs.Url, inputs.UserName, inputs.UserPassword, inputs.Path, true, tokenSource.Token);
}