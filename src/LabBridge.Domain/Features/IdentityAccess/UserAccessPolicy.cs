using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.IdentityAccess.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Domain.Features.IdentityAccess;

/// <summary>
/// User lifecycle and role-assignment rules (FR-IAM-010/011/014/015, BR-IAM-004/006/007/013), expressed on plain values.
/// ARCHITECTURE: these rules used to live inside the Domain "User" aggregate, which was an identity persistence entity.
/// The user entity is now ApplicationUser (ASP.NET Core Identity, Infrastructure). The rules stay in the Domain,
/// free of any Identity or EF dependency, and ApplicationUser delegates to them.
/// </summary>
public static class UserAccessPolicy
{
    public static void EnsureCanIssueInvitation(UserStatus status)
    {
        if (status != UserStatus.Invited)
            throw InvalidState("Invitations can only be issued to users in the Invited state.");
    }

    public static void EnsureCanAcceptInvitation(UserStatus status)
    {
        if (status != UserStatus.Invited)
            throw InvalidState("Only an invited user can accept an invitation.");
    }

    /// <param name="effectiveStatus">Status including lockout derived from Identity (see ApplicationUser.GetEffectiveStatus).</param>
    public static void EnsureCanSignIn(UserStatus effectiveStatus)
    {
        if (effectiveStatus != UserStatus.Active)
            throw InvalidState("Only an active user can sign in.");
    }

    /// <param name="isLastActiveAdministrator">Computed by the caller under the per-organization administrative lock (BR-IAM-006).</param>
    public static void EnsureCanDisable(
        Guid userId,
        Guid actorId,
        UserStatus status,
        bool isLastActiveAdministrator
    )
    {
        EnsureNotSelfModification(userId, actorId, "A user cannot disable themselves.");
        if (status == UserStatus.Disabled)
            throw InvalidState("The user is already disabled.");
        EnsureNotLastAdministrator(isLastActiveAdministrator);
    }

    /// <summary>Active when previously activated, otherwise back to Invited (a new invitation is then issued).</summary>
    public static UserStatus ResolveStatusAfterEnable(UserStatus status, DateTimeOffset? activatedAt)
    {
        if (status != UserStatus.Disabled)
            throw InvalidState("Only a disabled user can be enabled.");
        return activatedAt is null ? UserStatus.Invited : UserStatus.Active;
    }

    public static void EnsureCanUnlock(UserStatus effectiveStatus)
    {
        if (effectiveStatus != UserStatus.Locked)
            throw InvalidState("Only a locked user can be unlocked.");
    }

    /// <summary>
    /// Scope match (BR-IAM-004, VR-IAM-015), no self-escalation (BR-IAM-007), uniqueness and role compatibility (BR-IAM-013).
    /// </summary>
    public static void EnsureCanAssignRole(
        Guid userId,
        Guid actorId,
        UserStatus status,
        bool isPlatformUser,
        RoleSnapshot role,
        IReadOnlyCollection<RoleSnapshot> currentRoles
    )
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(currentRoles);

        EnsureNotSelfModification(userId, actorId, "A user cannot change their own roles.");
        if (status == UserStatus.Disabled)
            throw InvalidState("Roles cannot be assigned to a disabled user.");
        if (isPlatformUser != (role.Scope == RoleScope.Platform))
            throw new ValidationException(
                "ROLE_SCOPE_MISMATCH",
                "Role scope mismatch",
                "The role scope does not match the user scope."
            );

        // IMPROVEMENT: the duplicate check uses the roles supplied for the compatibility check, so it no longer depends on
        // a navigation collection that may not be loaded. The composite PK (UserId, RoleId) remains the final backstop.
        if (currentRoles.Any(r => r.Id == role.Id))
            throw new DuplicateResourceException(
                "USER_ROLE_ALREADY_ASSIGNED",
                "Role already assigned",
                "The user already holds this role."
            );

        RoleCompatibilityPolicy.EnsureCompatible(role, currentRoles);
    }

    public static void EnsureNotSelfModification(Guid userId, Guid actorId, string detail)
    {
        if (actorId == userId)
            throw new ForbiddenException(
                "USER_SELF_ESCALATION_DENIED",
                "Self-modification not allowed",
                detail
            );
    }

    /// <param name="wouldRemoveLastAdministrator">Computed by the caller under the per-organization administrative lock (BR-IAM-006).</param>
    public static void EnsureNotLastAdministrator(bool wouldRemoveLastAdministrator)
    {
        if (wouldRemoveLastAdministrator)
            throw new ConflictException(
                "USER_LAST_ADMIN",
                "Last administrator",
                "The organization must keep at least one administrator."
            );
    }

    private static BusinessRuleException InvalidState(string detail) =>
        new("USER_INVALID_STATE", "Invalid user state", detail);
}