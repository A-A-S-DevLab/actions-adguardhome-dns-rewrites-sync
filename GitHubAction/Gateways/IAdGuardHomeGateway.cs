namespace AdGuardHomeConnector.GitHubAction.Gateways;

public interface IAdGuardHomeGateway
{
    Task<List<DomainDto>> GetRewrites(string requestUrl, string userName, string userPassword, CancellationToken cancellationToken = default);

    Task AddRewrite(string requestUrl, string userName, string userPassword, DomainDto domainDto, CancellationToken cancellationToken = default);

    Task AddRewrite(string requestUrl, string userName, string userPassword, string domainDtoRequestContent, CancellationToken cancellationToken = default);

    Task DeleteRewrite(string requestUrl, string userName, string userPassword, DomainDto domainDto, CancellationToken cancellationToken = default);

    Task DeleteRewrite(string requestUrl, string userName, string userPassword, string domainDtoRequestContent, CancellationToken cancellationToken = default);
}