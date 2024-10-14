namespace AdGuardHomeConnector.GitHubAction.Extensions;

static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddGitHubActionServices(this IServiceCollection services) =>
        services.AddHttpClient()
            .AddTransient<IAdGuardHomeGateway, AdGuardHomeGateway>()
            .AddTransient<IAdGuardHomeService, AdGuardHomeService>();
        
}
