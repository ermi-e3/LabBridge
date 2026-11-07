namespace LabBridge.Domain.Common.Events;

/// <summary>A meaningful business occurrence recorded by an aggregate. Dispatch is an application/infrastructure concern.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

/// <summary>Base record for domain events.</summary>
public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
