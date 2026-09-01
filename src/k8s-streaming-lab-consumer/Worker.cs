using Confluent.Kafka;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConsumer<string, string> consumer, IConfiguration config)
    {
        _logger = logger;
        _consumer = consumer;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topic = _config["Kafka:Topic"];
        _consumer.Subscribe(topic);

        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    _logger.LogInformation("Consumed message: Key={Key} Value={Value} Partition={Partition} Offset={Offset}",
                        result.Message.Key, result.Message.Value, result.Partition, result.Offset);
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