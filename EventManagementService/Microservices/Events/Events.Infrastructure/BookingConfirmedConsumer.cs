using System.Text.Json;
using Confluent.Kafka;
using Contracts;
using Events.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure;

public sealed class BookingConfirmedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<BookingConfirmedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"];
        var consumerGroup = configuration["Kafka:ConsumerGroup"];

        if (string.IsNullOrWhiteSpace(bootstrapServers) || string.IsNullOrWhiteSpace(consumerGroup))
        {
            logger.LogError("Kafka:BootstrapServers and Kafka:ConsumerGroup must be configured.");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(KafkaTopics.BookingConfirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var message = JsonSerializer.Deserialize<BookingConfirmed>(result.Message.Value);
                    if (message is null)
                    {
                        logger.LogWarning("Kafka message at {TopicPartitionOffset} has an invalid BookingConfirmed body", result.TopicPartitionOffset);
                        continue;
                    }

                    using var scope = scopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var processingResult = await eventService.DecreaseAvailableSeatsAsync(message.EventId, message.SeatsCount);

                    if (processingResult == SeatsDecreaseResult.EventNotFound)
                        logger.LogWarning("Event {EventId} from booking {BookingId} was not found. Message skipped.", message.EventId, message.BookingId);
                    else if (processingResult == SeatsDecreaseResult.NotEnoughSeats)
                        logger.LogWarning("Event {EventId} has no free seats for booking {BookingId}. Message skipped.", message.EventId, message.BookingId);
                    else
                        logger.LogInformation("Seats were decreased for event {EventId} after booking {BookingId}", message.EventId, message.BookingId);
                }
                catch (ConsumeException ex)
                {
                    logger.LogWarning(ex, "Kafka consumer error");
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "Kafka message could not be deserialized as BookingConfirmed. Message skipped.");
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(ex, "Could not process BookingConfirmed message");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        finally
        {
            consumer.Close();
        }
    }
}
