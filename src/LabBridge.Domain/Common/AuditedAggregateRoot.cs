namespace LabBridge.Domain.Common;

/// <summary>Aggregate root carrying the standard creation/update audit stamps (UTC; actor null = system).</summary>
//public abstract class AuditedAggregateRoot : AggregateRoot
//{
//    public DateTimeOffset CreatedAt { get; private set; }
//    public Guid? CreatedBy { get; private set; }
//    public DateTimeOffset UpdatedAt { get; private set; }
//    public Guid? UpdatedBy { get; private set; }

//    protected void StampCreated(DateTimeOffset now, Guid? actorId)
//    {
//        CreatedAt = now;
//        CreatedBy = actorId;
//        UpdatedAt = now;
//        UpdatedBy = actorId;
//    }

//    protected void StampUpdated(DateTimeOffset now, Guid? actorId)
//    {
//        UpdatedAt = now;
//        UpdatedBy = actorId;
//    }
//}


/// <summary>Aggregate root carrying the standard creation/update audit stamps (UTC; actor null = system).</summary>
public abstract class AuditedAggregateRoot : AggregateRoot
{
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    protected void StampCreated(DateTimeOffset now, Guid? actorId)
    {
        // CHANGE: the summary promises UTC but the value was stored as given. Npgsql only accepts offset 0 for timestamptz, so a caller
        // passing DateTimeOffset.Now would fail at SaveChanges.
        var utc = now.ToUniversalTime();
        CreatedAt = utc;
        CreatedBy = actorId;
        UpdatedAt = utc;
        UpdatedBy = actorId;
    }

    protected void StampUpdated(DateTimeOffset now, Guid? actorId)
    {
        UpdatedAt = now.ToUniversalTime(); // CHANGE: same normalization as StampCreated
        UpdatedBy = actorId;
    }
}