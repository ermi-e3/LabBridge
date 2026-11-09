using System.Security.Cryptography;
using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.IdentityAccess;
using LabBridge.Domain.Features.IdentityAccess.Entities;
using LabBridge.Domain.Features.IdentityAccess.Enums;
using Microsoft.AspNetCore.Identity;

namespace LabBridge.Infrastructure.Identity.Entities;

/// <summary>
/// LabBridge user as an ASP.NET Core Identity user (organization user, or platform user when OrganizationId is null).
/// Never hard-deleted (BR-IAM-010).
/// ARCHITECTURE: replaces the Domain "User" aggregate. Identity persistence belongs in Infrastructure. Every business
/// rule is delegated to Domain policies (UserAccessPolicy, RoleCompatibilityPolicy), so this class only holds state.
/// CHANGE: user name, e-mail, password hash, lockout and security stamp now come from IdentityUser{Guid}. The former
/// EmailNormalized, LocalCredential and ExternalIdentity are replaced by NormalizedEmail, PasswordHash/lockout
/// fields and IdentityUserLogin (external issuer/subject).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    private readonly List<UserInvitation> _invitations = [];
    private readonly List<ApplicationUserRole> _roleAssignments = [];

    // Private: users are created only through Invite, so a user can never be persisted with an empty Guid key.
    private ApplicationUser() { }

    /// <summary>Null for platform users. Tenant-owned otherwise (SC-ORG-003); platform users are the documented exception.</summary>
    public Guid? OrganizationId { get; private set; }
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Persisted lifecycle status. Locked is never persisted: it is derived from Identity lockout (GetEffectiveStatus).
    /// </summary>
    public UserStatus Status { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }

    // Local-account extras that Identity has no equivalent for (conditional on DEC-004).
    public bool MustChangePassword { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }

    // IMPROVEMENT: audit stamps are duplicated from AuditedAggregateRoot because C# single inheritance prevents
    // deriving from both IdentityUser and the Domain base type.
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public bool IsPlatformUser => OrganizationId is null;
    public IReadOnlyCollection<UserInvitation> Invitations => _invitations.AsReadOnly();
    public IReadOnlyCollection<ApplicationUserRole> RoleAssignments => _roleAssignments.AsReadOnly();

    /// <summary>Creates a user in the Invited state (FR-IAM-010). Persist it with UserManager.CreateAsync.</summary>
    public static ApplicationUser Invite(
        Guid? organizationId,
        EmailAddress email,
        string displayName,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(email);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            // The e-mail address is the Identity user name; platform-wide uniqueness is BR-IAM-016.
            UserName = email.Value,
            Email = email.Value,
            DisplayName = Guard.Required(displayName, nameof(displayName), 200),
            Status = UserStatus.Invited,
            // SECURITY: lockout must apply to every user, even if the entity is persisted without UserManager.
            LockoutEnabled = true,
        };
        user.StampCreated(now, actorId);
        return user;
    }

    public void Rename(string displayName, DateTimeOffset now, Guid? actorId)
    {
        DisplayName = Guard.Required(displayName, nameof(displayName), 200);
        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Status including Identity lockout. CHANGE: lockout previously lived in LocalCredential and User.Lock(), a second
    /// source of truth. Identity's LockoutEnd (set by SignInManager/UserManager) is now authoritative.
    /// </summary>
    public UserStatus GetEffectiveStatus(DateTimeOffset now) =>
        Status == UserStatus.Active
        && LockoutEnabled
        && LockoutEnd is { } until
        && until > now
            ? UserStatus.Locked
            : Status;

    /// <summary>Issues a new invitation, revoking any outstanding one (BR-IAM-014). The user must be Invited.</summary>
    public UserInvitation IssueInvitation(
        byte[] tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        UserAccessPolicy.EnsureCanIssueInvitation(Status);

        // IMPROVEMENT: the new invitation is validated before outstanding ones are revoked, so a validation failure
        // cannot leave the in-memory aggregate half-modified.
        var invitation = UserInvitation.Issue(Id, OrganizationId, tokenHash, expiresAt, now, actorId);
        foreach (var open in _invitations)
            open.Revoke(now);
        _invitations.Add(invitation);
        return invitation;
    }

    /// <summary>
    /// Accepts an invitation: consumes it and activates the user (FR-IAM-011).
    /// CHANGE: the external identity is no longer bound here. The caller binds it with UserManager.AddLoginAsync
    /// (issuer = LoginProvider, subject = ProviderKey) inside the same database transaction.
    /// </summary>
    public void AcceptInvitation(Guid invitationId, DateTimeOffset now)
    {
        UserAccessPolicy.EnsureCanAcceptInvitation(Status);
        var invitation =
            _invitations.FirstOrDefault(i => i.Id == invitationId)
            ?? throw new ResourceNotFoundException(
                "INVITATION_NOT_FOUND",
                "Invitation not found",
                "The invitation does not exist for this user."
            );
        invitation.MarkUsed(now);
        Status = UserStatus.Active;
        ActivatedAt = now;
        StampUpdated(now, Id);
    }

    public void RecordLogin(DateTimeOffset now)
    {
        UserAccessPolicy.EnsureCanSignIn(GetEffectiveStatus(now));
        LastLoginAt = now;
    }

    /// <param name="isLastActiveAdministrator">Computed by the caller under the per-organization administrative lock (BR-IAM-006).</param>
    public void Disable(Guid actorId, bool isLastActiveAdministrator, DateTimeOffset now)
    {
        UserAccessPolicy.EnsureCanDisable(Id, actorId, Status, isLastActiveAdministrator);
        foreach (var invitation in _invitations)
            invitation.Revoke(now);
        Status = UserStatus.Disabled;
        DisabledAt = now;
        // SECURITY: rotating the security stamp invalidates cookies and tokens that validate the stamp, so a disabled
        // user does not keep a working session.
        RotateSecurityStamp();
        StampUpdated(now, actorId);
    }

    /// <summary>Re-enables a disabled user: Active when previously activated, otherwise back to Invited.</summary>
    public void Enable(DateTimeOffset now, Guid? actorId)
    {
        Status = UserAccessPolicy.ResolveStatusAfterEnable(Status, ActivatedAt);
        DisabledAt = null;
        StampUpdated(now, actorId);
    }

    /// <summary>Clears Identity lockout and the failed-attempt counter (replaces User.Unlock + LocalCredential.Unlock).</summary>
    public void Unlock(DateTimeOffset now, Guid? actorId)
    {
        UserAccessPolicy.EnsureCanUnlock(GetEffectiveStatus(now));
        LockoutEnd = null;
        AccessFailedCount = 0;
        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Call after UserManager has changed or reset the password. The hash itself is produced and verified by Identity's
    /// IPasswordHasher, never by custom code.
    /// </summary>
    public void RecordPasswordChange(DateTimeOffset now)
    {
        PasswordChangedAt = now;
        MustChangePassword = false;
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void RequirePasswordChange() => MustChangePassword = true;

    /// <summary>
    /// Assigns a role (FR-IAM-014/015). All rules are enforced by the Domain policy.
    /// SECURITY: role assignment must go through this method, not UserManager.AddToRoleAsync. The latter would bypass
    /// scope, self-escalation and compatibility rules. IdentityAccessDbContext rejects such rows at SaveChanges.
    /// </summary>
    public ApplicationUserRole AssignRole(
        RoleSnapshot role,
        IReadOnlyCollection<RoleSnapshot> currentRoles,
        Guid actorId,
        DateTimeOffset now
    )
    {
        UserAccessPolicy.EnsureCanAssignRole(Id, actorId, Status, IsPlatformUser, role, currentRoles);
        var assignment = new ApplicationUserRole(Id, OrganizationId, role.Id, now, actorId);
        _roleAssignments.Add(assignment);
        // SECURITY: refresh the stamp so role claims cached in cookies/tokens are re-evaluated.
        RotateSecurityStamp();
        StampUpdated(now, actorId);
        return assignment;
    }

    /// <remarks>Requires RoleAssignments to be loaded (Include) by the caller.</remarks>
    /// <param name="removesLastAdministrator">Computed by the caller under the per-organization administrative lock (BR-IAM-006).</param>
    public void RevokeRole(
        Guid roleId,
        Guid actorId,
        bool removesLastAdministrator,
        DateTimeOffset now
    )
    {
        UserAccessPolicy.EnsureNotSelfModification(Id, actorId, "A user cannot change their own roles.");
        var assignment =
            _roleAssignments.FirstOrDefault(a => a.RoleId == roleId)
            ?? throw new ResourceNotFoundException(
                "USER_ROLE_NOT_ASSIGNED",
                "Role assignment not found",
                "The user does not hold this role."
            );
        UserAccessPolicy.EnsureNotLastAdministrator(removesLastAdministrator);
        _roleAssignments.Remove(assignment);
        RotateSecurityStamp();
        StampUpdated(now, actorId);
    }

    private void RotateSecurityStamp() =>
        SecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(20));

    private void StampCreated(DateTimeOffset now, Guid? actorId)
    {
        CreatedAt = now;
        CreatedBy = actorId;
        UpdatedAt = now;
        UpdatedBy = actorId;
    }

    private void StampUpdated(DateTimeOffset now, Guid? actorId)
    {
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}