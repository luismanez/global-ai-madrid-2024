using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;

namespace CozyKitchen.HostedServices;
public class EmbeddingGenerationHostedService : IHostedService
{
    private readonly ITextEmbeddingGeneration _textEmbeddingGeneration;
    private readonly ILogger<EmbeddingGenerationHostedService> _logger;

    public EmbeddingGenerationHostedService(
        IKernel kernel,
        ILogger<EmbeddingGenerationHostedService> logger)
    {
        _textEmbeddingGeneration = kernel.GetService<ITextEmbeddingGeneration>();
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var embeddings = await _textEmbeddingGeneration.GenerateEmbeddingsAsync(new []{ "Hi" });
        var embedding = embeddings[0];

        _logger.LogInformation($"Embeddings for 'Hi' string:");
        var result = string.Join(",", embedding.Span.ToArray());
        _logger.LogInformation(result);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("HostedService Stopped");
        return Task.CompletedTask;
    }
}
