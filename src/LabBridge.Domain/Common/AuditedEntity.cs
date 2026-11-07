namespace LabBridge.Domain.Common;

/// <summary>Child entity carrying the standard creation/update audit stamps.</summary>
public abstract class AuditedEntity : Entity
{
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    protected void StampCreated(DateTimeOffset now, Guid? actorId)
    {
        CreatedAt = now;
        CreatedBy = actorId;
        UpdatedAt = now;
        UpdatedBy = actorId;
    }

    protected void StampUpdated(DateTimeOffset now, Guid? actorId)
    {
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}
