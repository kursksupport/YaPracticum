using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure;

public sealed class KafkaTopicInitializer(
    IConfiguration configuration,
    ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            logger.LogWarning("Kafka:BootstrapServers is not configured. Topic will not be created.");
            return;
        }

        var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        try
        {
            await adminClient.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = KafkaTopics.BookingConfirmed,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            ]).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            logger.LogInformation("Kafka topic {Topic} was created", KafkaTopics.BookingConfirmed);
        }
        catch (CreateTopicsException ex) when (ex.Results.All(x => x.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Kafka topic {Topic} already exists", KafkaTopics.BookingConfirmed);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not create Kafka topic {Topic}. The consumer will continue to retry connection.", KafkaTopics.BookingConfirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
