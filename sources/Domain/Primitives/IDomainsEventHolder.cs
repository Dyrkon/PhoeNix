namespace Domain.Primitives;

public interface IDomainsEventHolder
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();

    void RaiseDomainEvent(IDomainEvent domainEvent);
}