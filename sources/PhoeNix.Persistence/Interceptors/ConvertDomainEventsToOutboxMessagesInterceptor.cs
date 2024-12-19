using Domain.Primitives;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using PhoeNix.Persistence.Outbox;

namespace PhoeNix.Persistence.Interceptors;

public class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var dbContext = eventData.Context;

        if (dbContext is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var outboxMessages = dbContext.ChangeTracker.Entries<IDomainsEventHolder>()
            .Select(x => x.Entity)
            .SelectMany(
                domainHolder =>
                {
                    var domainEvents = domainHolder.GetDomainEvents();
                    domainHolder.ClearDomainEvents();
                    return domainEvents;
                })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),

                OccurredOnUtc = DateTime.Now,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })
            })
            .ToList();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}