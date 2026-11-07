using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.IdentityAccess.Entities;

namespace LabBridge.Domain.Features.IdentityAccess;

/// <summary>
/// Domain service for role-combination rules that span several Role aggregates (BR-IAM-009, BR-IAM-013).
/// ASSUMPTION A-IAM-01: because the set of "state-changing" permissions is not enumerated in the FRS, the Auditor role is
/// treated as incompatible with every other non-additive role (Proposed DEC-028; owner may waive).
/// </summary>
//public static class RoleCompatibilityPolicy
//{
//    public static void EnsureCompatible(Role candidate, IEnumerable<Role> existing)
//    {
//        var current = existing.ToList();
//        var candidateIsAuditor = candidate.Code == SystemRoleCodes.Auditor;

//        if (candidate.IsAdditive && current.All(r => r.IsAdditive))
//            throw new ValidationException(
//                "ROLE_ADDITIVE_REQUIRES_PRIMARY",
//                "Primary role required",
//                "An additive role can only be held together with a primary role."
//            );

//        var conflicts = candidateIsAuditor
//            ? current.Any(r => !r.IsAdditive)
//            : !candidate.IsAdditive && current.Any(r => r.Code == SystemRoleCodes.Auditor);
//        if (conflicts)
//            throw new ValidationException(
//                "ROLE_INCOMPATIBLE",
//                "Incompatible roles",
//                "The Auditor role cannot be combined with roles that grant state-changing permissions."
//            );
//    }
//}



/// <summary>
/// Domain service for role-combination rules that span several roles (BR-IAM-009, BR-IAM-013).
/// ASSUMPTION A-IAM-01: because the set of "state-changing" permissions is not enumerated in the FRS, the Auditor role is
/// treated as incompatible with every other non-additive role (Proposed DEC-028; owner may waive).
/// </summary>
public static class RoleCompatibilityPolicy
{
    // ARCHITECTURE: operates on RoleSnapshot instead of the former Role entity, which moved to Infrastructure
    // as an ASP.NET Core Identity role. The rules themselves are unchanged.
    public static void EnsureCompatible(RoleSnapshot candidate, IEnumerable<RoleSnapshot> existing)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existing);

        var current = existing.ToList();
        var candidateIsAuditor = candidate.Code == SystemRoleCodes.Auditor;

        if (candidate.IsAdditive && current.All(r => r.IsAdditive))
            throw new ValidationException(
                "ROLE_ADDITIVE_REQUIRES_PRIMARY",
                "Primary role required",
                "An additive role can only be held together with a primary role."
            );

        var conflicts = candidateIsAuditor
            ? current.Any(r => !r.IsAdditive)
            : !candidate.IsAdditive && current.Any(r => r.Code == SystemRoleCodes.Auditor);
        if (conflicts)
            throw new ValidationException(
                "ROLE_INCOMPATIBLE",
                "Incompatible roles",
                "The Auditor role cannot be combined with roles that grant state-changing permissions."
            );
    }
}