using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AdGuardHomeConnector.GitHubAction.Gateways;

public class AdGuardHomeGateway : IAdGuardHomeGateway
{
    private const string GET_REWRITE_PATH = "/control/rewrite/list";
    private const string ADD_REWRITE_PATH = "/control/rewrite/add";
    private const string DELETE_REWRITE_PATH = "/control/rewrite/delete";

    private readonly ILogger<AdGuardHomeGateway> _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public AdGuardHomeGateway(
        ILogger<AdGuardHomeGateway> logger,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<List<DomainDto>> GetRewrites(string requestUrl, string userName, string userPassword, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri($"{requestUrl}{GET_REWRITE_PATH}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{userPassword}")));
        
        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                _logger.LogError("Error: Authentication failed or access forbidden. Please check your AdGuard credentials, and permissions.");
                break;
            case HttpStatusCode.OK:
                break;
            default:
                _logger.LogError($"Error: Failed to fetch existing DNS entries from {requestUrl}: {responseContent}");
                break;
        }

        response.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<List<DomainDto>>(responseContent);
    }

    public Task AddRewrite(string requestUrl, string userName, string userPassword, DomainDto domainDto, CancellationToken cancellationToken = default)
    {
        return AddRewrite(requestUrl, userName, userPassword, JsonConvert.SerializeObject(domainDto), cancellationToken);
    }
    
    public async Task AddRewrite(string requestUrl, string userName, string userPassword, string domainDtoRequestContent, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri($"{requestUrl}{ADD_REWRITE_PATH}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{userPassword}")));
        request.Content = new StringContent(domainDtoRequestContent, Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        
        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                _logger.LogError("Error: Authentication failed or access forbidden. Please check your AdGuard credentials, and permissions.");
                break;
            case HttpStatusCode.OK:
                _logger.LogInformation($"Added '{domainDtoRequestContent}' on {requestUrl}");
                break;
            default:
                _logger.LogError($"Error: Failed to add '{domainDtoRequestContent}' on {requestUrl}: {responseContent}");
                break;
        }

        response.EnsureSuccessStatusCode();
    }

    public Task DeleteRewrite(string requestUrl, string userName, string userPassword, DomainDto domainDto, CancellationToken cancellationToken = default)
    {
        return DeleteRewrite(requestUrl, userName, userPassword, JsonConvert.SerializeObject(domainDto), cancellationToken);
    }
    
    public async Task DeleteRewrite(string requestUrl, string userName, string userPassword, string domainDtoRequestContent, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri($"{requestUrl}{DELETE_REWRITE_PATH}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{userPassword}")));
        request.Content = new StringContent(domainDtoRequestContent, Encoding.UTF8, "application/json");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                _logger.LogError("Error: Authentication failed or access forbidden. Please check your AdGuard credentials, and permissions.");
                break;
            case HttpStatusCode.OK:
                _logger.LogInformation($"Deleted '{domainDtoRequestContent}' on {requestUrl}");
                break;
            default:
                _logger.LogError($"Error: Failed to delete '{domainDtoRequestContent}' on {requestUrl}: {responseContent}");
                break;
        }

        response.EnsureSuccessStatusCode();
    }
}