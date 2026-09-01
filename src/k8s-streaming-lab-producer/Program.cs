using Confluent.Kafka;
using k8s_streaming_lab_producer;


var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
