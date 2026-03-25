using PhoeNix.Domain.Primitives;

namespace PhoeNix.Application.Abstractions.Outbox;

public interface IOutboxMessageSerializer
{
    string Serialize(IDomainEvent domainEvent);
    IDomainEvent Deserialize(string type, string content);
}