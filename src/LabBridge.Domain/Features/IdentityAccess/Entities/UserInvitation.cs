using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Features.IdentityAccess.Entities;

/// <summary>Single-use, expiring, revocable invitation. Only a hash of the high-entropy token is stored; the plaintext is never persisted (BR-IAM-014).</summary>
//public sealed class UserInvitation : Entity
//{
//    private UserInvitation() { }

//    internal UserInvitation(
//        Guid userId,
//        Guid? organizationId,
//        byte[] tokenHash,
//        DateTimeOffset expiresAt,
//        DateTimeOffset now,
//        Guid? createdBy
//    )
//    {
//        if (expiresAt <= now)
//            throw new ValidationException(
//                "INVITATION_EXPIRY_INVALID",
//                "Invalid invitation expiry",
//                "The invitation expiry must be in the future."
//            );
//        Id = Guid.NewGuid();
//        UserId = Guard.NotEmpty(userId, nameof(userId));
//        OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId));
//        TokenHash = Guard.RequiredBytes(tokenHash, nameof(tokenHash));
//        ExpiresAt = expiresAt;
//        CreatedAt = now;
//        CreatedBy = createdBy;
//    }

//    public Guid UserId { get; private set; }
//    public Guid? OrganizationId { get; private set; }
//    public byte[] TokenHash { get; private set; } = null!;
//    public DateTimeOffset ExpiresAt { get; private set; }
//    public DateTimeOffset? UsedAt { get; private set; }
//    public DateTimeOffset? RevokedAt { get; private set; }
//    public DateTimeOffset CreatedAt { get; private set; }
//    public Guid? CreatedBy { get; private set; }

//    public bool IsUsable(DateTimeOffset now) =>
//        UsedAt is null && RevokedAt is null && ExpiresAt > now;

//    internal void MarkUsed(DateTimeOffset now)
//    {
//        if (!IsUsable(now))
//            throw new BusinessRuleException(
//                "INVITATION_NOT_USABLE",
//                "Invitation not usable",
//                "The invitation is expired, revoked or already used."
//            );
//        UsedAt = now;
//    }

//    internal void Revoke(DateTimeOffset now)
//    {
//        if (UsedAt is null && RevokedAt is null)
//            RevokedAt = now;
//    }
//}


/// <summary>Single-use, expiring, revocable invitation. Only a hash of the high-entropy token is stored; the plaintext is never persisted (BR-IAM-014).</summary>
public sealed class UserInvitation : Entity
{
    private UserInvitation() { }

    // CHANGE: the former internal constructor was only callable from the Domain User aggregate. The user is now an
    // Identity entity in Infrastructure (a different assembly), so creation is exposed as a public factory.
    // The user is referenced by identifier only (ARCHITECTURE).
    public static UserInvitation Issue(
        Guid userId,
        Guid? organizationId,
        byte[] tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset now,
        Guid? createdBy
    )
    {
        if (expiresAt <= now)
            throw new ValidationException(
                "INVITATION_EXPIRY_INVALID",
                "Invalid invitation expiry",
                "The invitation expiry must be in the future."
            );
        return new UserInvitation
        {
            Id = Guid.NewGuid(),
            UserId = Guard.NotEmpty(userId, nameof(userId)),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            TokenHash = Guard.RequiredBytes(tokenHash, nameof(tokenHash)),
            ExpiresAt = expiresAt,
            CreatedAt = now,
            CreatedBy = createdBy,
        };
    }

    public Guid UserId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public byte[] TokenHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public bool IsUsable(DateTimeOffset now) =>
        UsedAt is null && RevokedAt is null && ExpiresAt > now;

    // CHANGE: internal -> public (see Issue). Callers should go through ApplicationUser.AcceptInvitation, which also
    // checks the user's status. Single use under concurrency is still enforced by the conditional update at the
    // persistence boundary (UsedAt IS NULL).
    public void MarkUsed(DateTimeOffset now)
    {
        if (!IsUsable(now))
            throw new BusinessRuleException(
                "INVITATION_NOT_USABLE",
                "Invitation not usable",
                "The invitation is expired, revoked or already used."
            );
        UsedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        if (UsedAt is null && RevokedAt is null)
            RevokedAt = now;
    }
}