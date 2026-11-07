using LabBridge.Domain.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Identity;

/// <summary>
/// Assignment of a role to a user (canonical authorization data). OrganizationId mirrors the user's organization
/// (null for platform users).
/// CHANGE: replaces Domain UserRoleAssignment. A custom join type is justified because the module records
/// AssignedAt / AssignedBy / OrganizationId, which IdentityUserRole{Guid} does not carry. The composite key
/// (UserId, RoleId) from Identity replaces the former surrogate Id plus unique index.
/// </summary>
public sealed class ApplicationUserRole : IdentityUserRole<Guid>
{
    /// <summary>
    /// Required by Identity's UserStore (new() constraint on the user-role type). Rows created this way carry no audit
    /// data, and IdentityAccessDbContext rejects them at SaveChanges. Use ApplicationUser.AssignRole instead.
    /// </summary>
    public ApplicationUserRole() { }

    internal ApplicationUserRole(
        Guid userId,
        Guid? organizationId,
        Guid roleId,
        DateTimeOffset now,
        Guid? assignedBy
    )
    {
        UserId = Guard.NotEmpty(userId, nameof(userId));
        OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId));
        RoleId = Guard.NotEmpty(roleId, nameof(roleId));
        AssignedAt = now;
        AssignedBy = assignedBy;
    }

    public Guid? OrganizationId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }
}