using System.Text.Json;
using Bookings.Application;
using Confluent.Kafka;
using Contracts;
using Microsoft.Extensions.Configuration;

namespace Bookings.Infrastructure;

public sealed class KafkaBookingConfirmedPublisher : IBookingConfirmedPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaBookingConfirmedPublisher(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers is not configured.");
        _producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build();
    }

    public async Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync(KafkaTopics.BookingConfirmed, new Message<string, string>
        {
            Key = message.EventId.ToString(),
            Value = JsonSerializer.Serialize(message)
        }, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
