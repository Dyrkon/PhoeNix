using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PhoeNix.Application.Abstractions.Outbox;
using PhoeNix.Application.Models.Outbox;
using PhoeNix.Domain.Primitives;
using PhoeNix.Persistence.Outbox;

namespace PhoeNix.Persistence.Interceptors;

internal sealed class InsertOutboxMessagesInterceptor(IOutboxMessageSerializer outboxMessageSerializer)
    : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<DbContext, PendingOutboxState> _pendingStates = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        InsertOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        InsertOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        RollbackPendingState(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RollbackPendingState(eventData.Context);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void InsertOutboxMessages(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        if (_pendingStates.TryGetValue(dbContext, out _))
            return;

        var aggregatesWithEvents = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        if (aggregatesWithEvents.Count == 0)
            return;

        var outboxMessages = aggregatesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .Select(domainEvent => OutboxMessage.Create(
                DateTime.UtcNow,
                domainEvent.GetType().AssemblyQualifiedName!,
                outboxMessageSerializer.Serialize(domainEvent)))
            .ToList();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

        _pendingStates.Add(dbContext, new PendingOutboxState(aggregatesWithEvents, outboxMessages));
    }

    private void ClearDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        if (!_pendingStates.TryGetValue(dbContext, out var pendingState))
            return;

        foreach (var aggregate in pendingState.Aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        _pendingStates.Remove(dbContext);
    }

    private void RollbackPendingState(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        if (!_pendingStates.TryGetValue(dbContext, out var pendingState))
            return;

        foreach (var outboxMessage in pendingState.OutboxMessages)
        {
            var entry = dbContext.Entry(outboxMessage);
            if (entry.State != EntityState.Detached)
                entry.State = EntityState.Detached;
        }

        _pendingStates.Remove(dbContext);
    }

    private sealed record PendingOutboxState(
        IReadOnlyCollection<IHasDomainEvents> Aggregates,
        IReadOnlyCollection<OutboxMessage> OutboxMessages);
}