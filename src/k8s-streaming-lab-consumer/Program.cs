using Confluent.Kafka;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        GroupId = builder.Configuration["Kafka:GroupId"],
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit=false
        
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();