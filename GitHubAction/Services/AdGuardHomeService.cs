using Newtonsoft.Json;

namespace AdGuardHomeConnector.GitHubAction.Services;

public class AdGuardHomeService : IAdGuardHomeService
{
    private const string FILE_EXTENSION = ".json";
    
    private readonly ILogger<AdGuardHomeGateway> _logger;

    private readonly IAdGuardHomeGateway _adGuardHomeGateway;

    public AdGuardHomeService(
        ILogger<AdGuardHomeGateway> logger,
        IAdGuardHomeGateway adGuardHomeGateway
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _adGuardHomeGateway = adGuardHomeGateway ?? throw new ArgumentNullException(nameof(adGuardHomeGateway));
    }
    
    public Task SyncRewrites(string requestUrl, string userName, string userPassword, string path, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default)
    {
        if (!Path.Exists(path))
        {
            throw new DirectoryNotFoundException($"Error: Path does not exist: {path}");
        }

        var attr = File.GetAttributes(path);

        return attr.HasFlag(FileAttributes.Directory)
            ? SyncRewritesFromDirectory(requestUrl, userName, userPassword, path, ignoreInvalidLines, cancellationToken)
            : SyncRewritesFromFile(requestUrl, userName, userPassword, path, ignoreInvalidLines, cancellationToken);
    }
    
    private async Task SyncRewritesFromDirectory(string requestUrl, string userName, string userPassword, string directory, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Error: Directory does not exist: {directory}");
        }

        var files = Directory.GetFiles(directory, $"*{FILE_EXTENSION}", SearchOption.AllDirectories);

        var domainsNew = new List<DomainDto>();
        foreach (var filename in files)
        {
            domainsNew.AddRange(await ReadFile(filename, ignoreInvalidLines, cancellationToken));
        }

        await SyncRewrites(requestUrl, userName, userPassword, domainsNew, ignoreInvalidLines, cancellationToken);
    }
    
    private async Task SyncRewritesFromFile(string requestUrl, string userName, string userPassword, string filename, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default)
    {
        var domainsNew = await ReadFile(filename, ignoreInvalidLines, cancellationToken);

        await SyncRewrites(requestUrl, userName, userPassword, domainsNew, ignoreInvalidLines, cancellationToken);
    }
    
    private async Task<List<DomainDto>> ReadFile(string filename, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"Error: File does not exist: {filename}");
        }
        if (!Path.HasExtension(filename))
        {
            throw new FileLoadException("Error: Invalid file format. No file extension");
        }
        if (Path.GetExtension(filename) != FILE_EXTENSION)
        {
            throw new FileLoadException($"Error: Invalid file extension: {Path.GetExtension(filename)}");
        }

        var fileContent = await File.ReadAllLinesAsync(filename, cancellationToken);

        var result = new List<DomainDto>();
        foreach (var line in fileContent.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            try
            {
                result.Add(JsonConvert.DeserializeObject<DomainDto>(line));
            }
            catch
            {
                if (!ignoreInvalidLines)
                {
                    throw new InvalidDataException($"Error: Line has invalid format: '{line}'");
                }
                
                _logger.LogWarning($"Error: line has incorrect format: '{line}'");
            }
        }

        return result;
    }
    
    private async Task SyncRewrites(string requestUrl, string userName, string userPassword, List<DomainDto> domainsNew, bool ignoreInvalidLines = true, CancellationToken cancellationToken = default)
    {
        var domainsOld = await _adGuardHomeGateway.GetRewrites(requestUrl, userName, userPassword, cancellationToken);

        foreach (var domainNew in domainsNew)
        {
            try
            {
                var existedDomain = domainsOld.FirstOrDefault(x => x.Domain == domainNew.Domain);
                
                if(existedDomain == null)
                {
                    await _adGuardHomeGateway.AddRewrite(requestUrl, userName, userPassword, domainNew, cancellationToken);
                
                    continue;
                }

                if (existedDomain.Answer == domainNew.Answer)
                {
                    _logger.LogInformation($"Already exist'{JsonConvert.SerializeObject(domainNew)}' on {requestUrl}");
                    
                    continue;
                }
            
                _logger.LogInformation($"Update '{JsonConvert.SerializeObject(domainNew)}' on {requestUrl}");
                await _adGuardHomeGateway.DeleteRewrite(requestUrl, userName, userPassword, domainNew, cancellationToken);
                await _adGuardHomeGateway.AddRewrite(requestUrl, userName, userPassword, domainNew, cancellationToken);
            }
            catch (Exception e)
            {
                if (!ignoreInvalidLines)
                {
                    throw;
                }
                    
                _logger.LogWarning($"Error: Domain update error: '{e.Message}'");
            }
        }

        foreach (var domainOld in domainsOld)
        {
            try
            {
                if(domainsNew.All(x => x.Domain != domainOld.Domain))
                {
                    await _adGuardHomeGateway.DeleteRewrite(requestUrl, userName, userPassword, domainOld, cancellationToken);
                }
            }
            catch (Exception e)
            {
                if (!ignoreInvalidLines)
                {
                    throw;
                }
                    
                _logger.LogWarning($"Error: Domain update error: '{e.Message}'");
            }
        }
    }
}