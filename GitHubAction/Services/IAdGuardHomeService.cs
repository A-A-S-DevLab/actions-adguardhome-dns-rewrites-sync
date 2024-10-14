namespace AdGuardHomeConnector.GitHubAction.Services;

public interface IAdGuardHomeService
{
    Task SyncRewrites(string requestUrl, string userName, string userPassword, string path, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default);
}