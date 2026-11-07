using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement;

/// <summary>Operating policy derived from organization status and suspension mode (BR-ORG-008). Never persisted.</summary>
public sealed record OrganizationOperatingPolicy(
    bool AllowsUserAccess,
    bool AllowsInboundAcceptance,
    bool AllowsInboundProcessing,
    bool AllowsOutboundDelivery
)
{
    private static readonly OrganizationOperatingPolicy Denied = new(false, false, false, false);

    public static OrganizationOperatingPolicy For(
        OrganizationStatus status,
        SuspensionMode? mode
    ) =>
        status switch
        {
            OrganizationStatus.Active => new(true, true, true, true),
            OrganizationStatus.Suspended when mode == SuspensionMode.HoldAndPreserve => new(
                false,
                true,
                false,
                false
            ),
            _ => Denied,
        };
}
