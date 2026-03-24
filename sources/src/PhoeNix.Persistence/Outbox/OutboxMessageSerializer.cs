using System.Text.Json;
using PhoeNix.Application.Abstractions.Outbox;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Persistence.Outbox;

internal sealed class OutboxMessageSerializer(JsonSerializerOptions jsonSerializerOptions) : IOutboxMessageSerializer
{
    public string Serialize(IDomainEvent domainEvent)
    {
        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), jsonSerializerOptions);
    }

    public IDomainEvent Deserialize(string type, string content)
    {
        var eventType = Type.GetType(type);
        if (eventType is null)
            throw new InvalidOperationException($"Outbox message type '{type}' could not be resolved.");

        var deserialized = JsonSerializer.Deserialize(content, eventType, jsonSerializerOptions);
        if (deserialized is not IDomainEvent domainEvent)
            throw new InvalidOperationException($"Outbox message type '{type}' did not deserialize to a domain event.");

        return domainEvent;
    }
}