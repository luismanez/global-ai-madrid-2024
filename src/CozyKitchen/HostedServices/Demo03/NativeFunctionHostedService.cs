using Azure.Identity;
using CozyKitchen.Plugins.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.SemanticKernel;

namespace CozyKitchen.HostedServices;
public class NativeFunctionHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IKernel _kernel;

    public NativeFunctionHostedService(
        ILogger<NativeFunctionHostedService> logger,
        IConfiguration configuration,
        IKernel kernel)
    {
        _logger = logger;
        _configuration = configuration;
        _kernel = kernel;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var graphClient = GetGraphServiceClient();

        var graphSkillsPlugin = new GraphUserProfileSkillsPlugin(graphClient);
        var skills = _kernel.ImportFunctions(graphSkillsPlugin);

        var mySkillsResult = await _kernel.RunAsync(string.Empty, skills["GetMySkills"]);

        var mySkills = mySkillsResult.GetValue<string>();

        _logger.LogInformation($"My Skills: {mySkills}");
    }

    private GraphServiceClient GetGraphServiceClient()
    {
        var scopes = new[] { "User.Read" };
        var clientId = _configuration.GetValue<string>("AzureAd:ClientId");
        var tenantId = _configuration.GetValue<string>("AzureAd:TenantId");

        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            RedirectUri = new Uri("http://localhost"),
        };

        var interactiveCredential = new InteractiveBrowserCredential(options);
        var graphClient = new GraphServiceClient(interactiveCredential, scopes);

        return graphClient;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("HostedService Stopped");
        return Task.CompletedTask;
    }
}
