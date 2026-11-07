using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>One requested test of an order. Codes are Confidential. Canonical and instrument codes are set by mapping.</summary>
public sealed class OrderLine : Entity, IOrganizationOwned
{
    private OrderLine() { }

    internal OrderLine(
        Guid organizationId,
        Guid orderId,
        int lineNumber,
        string externalTestCode,
        string? externalCodeSystem,
        string? description
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        OrderId = orderId;
        LineNumber = Guard.InRange(lineNumber, 1, int.MaxValue, nameof(lineNumber));
        ExternalTestCode = Guard.Required(externalTestCode, nameof(externalTestCode), 100);
        ExternalCodeSystem = Guard.Optional(externalCodeSystem, nameof(externalCodeSystem), 100);
        Description = Guard.Optional(description, nameof(description), 500);
    }

    public Guid OrganizationId { get; private set; }
    public Guid OrderId { get; private set; }
    public int LineNumber { get; private set; }
    public string ExternalTestCode { get; private set; } = null!;
    public string? ExternalCodeSystem { get; private set; }
    public string? Description { get; private set; }
    public string? CanonicalTestCode { get; private set; }
    public string? InstrumentTestCode { get; private set; }

    internal void ApplyMapping(string? canonicalTestCode, string? instrumentTestCode)
    {
        CanonicalTestCode = Guard.Optional(canonicalTestCode, nameof(canonicalTestCode), 100);
        InstrumentTestCode = Guard.Optional(instrumentTestCode, nameof(instrumentTestCode), 100);
    }
}
