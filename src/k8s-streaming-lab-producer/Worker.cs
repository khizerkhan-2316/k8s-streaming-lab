namespace k8s_streaming_lab_producer;

using Confluent.Kafka;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IProducer<string, string> producer, IConfiguration config)
    {
        _logger = logger;
        _producer = producer;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topic = _config["Kafka:Topic"];
        var counter = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = $"Hello Kafka #{counter++} at {DateTimeOffset.Now}"
            };

            try
            {
                var result = await _producer.ProduceAsync(topic, message, stoppingToken);
                _logger.LogInformation("Produced message to {Topic} partition {Partition} offset {Offset}",
                    result.Topic, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to produce message: {Reason}", ex.Error.Reason);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        await base.StopAsync(cancellationToken);
    }
}