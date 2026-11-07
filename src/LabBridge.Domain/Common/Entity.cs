namespace LabBridge.Domain.Common;

/// <summary>Base type for all persistent domain entities. Identifiers are UUIDs assigned by the domain (ASM-010: v4 vs time-ordered is an infrastructure decision).</summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    public bool Equals(Entity? other) => other is not null && GetType() == other.GetType() && Id != Guid.Empty && Id == other.Id;

    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
