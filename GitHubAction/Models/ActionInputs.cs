namespace AdGuardHomeConnector.GitHubAction.Models;

public class ActionInputs
{
    [Option('r', "path",
        Required = true,
        HelpText = "Path to the JSON file or directory with JSON files containing the DNS rewrites.")]
    public string Path { get; set; } = null!;

    [Option('n', "username",
        Required = true,
        HelpText = "AdGuardHome user name.")]
    public string UserName { get; set; } = null!;

    [Option('p', "userpassword",
        Required = true,
        HelpText = "AdGuardHome user password.")]
    public string UserPassword { get; set; } = null!;

    [Option('u', "url",
        Required = true,
        HelpText = "AdGuardHome URL (http//0.0.0.0:80).")]
    public string Url { get; set; } = null!;
}
