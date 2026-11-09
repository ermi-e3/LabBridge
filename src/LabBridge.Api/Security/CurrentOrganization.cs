using LabBridge.Application.Common.Organizations;
using System.Security.Claims;

namespace LabBridge.Api.Security;
public sealed class CurrentOrganization(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentOrganization
{
    private const string OrganizationClaim = "organization_id";

    public Guid? OrganizationId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var value = principal.FindFirstValue(OrganizationClaim);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Guid.TryParse(value, out var organizationId)
                && organizationId != Guid.Empty
                    ? organizationId
                    : throw new InvalidOperationException(
                        "The authenticated organization claim is invalid.");
        }
    }

    public bool IsOrganizationContextAvailable =>
        OrganizationId.HasValue;
}