using Confluent.Kafka;
using OpenSearch.Client;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IOpenSearchClient _openSearchClient;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConsumer<string, string> consumer,
        IOpenSearchClient openSearchClient, IConfiguration config)
    {
        _logger = logger;
        _consumer = consumer;
        _openSearchClient = openSearchClient;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topic = _config["Kafka:Topic"];
        var indexName = _config["OpenSearch:IndexName"];
        _consumer.Subscribe(topic);

        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    var doc = new StreamedMessage
                    {
                        Key = result.Message.Key,
                        Value = result.Message.Value,
                        Partition = result.Partition.Value,
                        Offset = result.Offset.Value,
                        ConsumedAt = DateTimeOffset.UtcNow
                    };

                    var response = await _openSearchClient.IndexAsync(doc, i => i.Index(indexName), stoppingToken);

                    if (response.IsValid)
                    {
                        _logger.LogInformation("Indexed message Offset={Offset} into OpenSearch index {Index}",
                            result.Offset, indexName);
                    }
                    else
                    {
                        _logger.LogError("Failed to index message: {Error}", response.DebugInformation);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Consume error: {Reason}", ex.Error.Reason);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        await base.StopAsync(cancellationToken);
    }
}

public class StreamedMessage
{
    public string Key { get; set; }
    public string Value { get; set; }
    public int Partition { get; set; }
    public long Offset { get; set; }
    public DateTimeOffset ConsumedAt { get; set; }
}