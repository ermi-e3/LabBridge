using LabBridge.Domain.Common;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>Result of a connection test for a connector configuration version. Tests send no clinical data (BR-CON-019). Insert-only.</summary>
public sealed class ConnectorTestResult : Entity, IOrganizationOwned
{
    private ConnectorTestResult() { }

    internal ConnectorTestResult(
        Guid organizationId,
        Guid connectorId,
        int configVersion,
        ConnectorTestOutcome outcome,
        string? summary,
        string? errorCode,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        Guid? testedBy
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ConnectorId = connectorId;
        ConfigVersion = configVersion;
        Outcome = Guard.Defined(outcome, nameof(outcome));
        Summary = Guard.Optional(summary, nameof(summary), 500);
        ErrorCode = Guard.Optional(errorCode, nameof(errorCode), 50);
        StartedAt = startedAt;
        CompletedAt = completedAt;
        TestedBy = testedBy;
    }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public int ConfigVersion { get; private set; }
    public ConnectorTestOutcome Outcome { get; private set; }

    /// <summary>Sanitized: no secrets, no patient data.</summary>
    public string? Summary { get; private set; }
    public string? ErrorCode { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }
    public Guid? TestedBy { get; private set; }
}
