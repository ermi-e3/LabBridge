using LabBridge.Domain.Common;
using LabBridge.Domain.Features.Audit.Enums;

namespace LabBridge.Domain.Features.Audit.Entities;

/// <summary>
/// Registered audit action (platform reference data seeded by migration; never deleted, only deactivated). The natural key is the action code
/// (for example "security.access.denied").
/// </summary>
public sealed class AuditActionDefinition
{
    private AuditActionDefinition() { }

    public string ActionCode { get; private set; } = null!;
    public string Domain { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public AuditActionClass ActionClass { get; private set; }
    public EmissionMode FailureMode { get; private set; }
    public string? DetailSchemaJson { get; private set; }
    public bool IsTraced { get; private set; }
    public string? TraceStage { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static AuditActionDefinition Define(
        string actionCode,
        string domain,
        string description,
        AuditActionClass actionClass,
        EmissionMode failureMode,
        string? detailSchemaJson,
        bool isTraced,
        string? traceStage,
        DateTimeOffset now
    ) =>
        new()
        {
            ActionCode = Guard.Required(actionCode, nameof(actionCode), 100),
            Domain = Guard.Required(domain, nameof(domain), 50),
            Description = Guard.Required(description, nameof(description), 500),
            ActionClass = Guard.Defined(actionClass, nameof(actionClass)),
            FailureMode = Guard.Defined(failureMode, nameof(failureMode)),
            DetailSchemaJson = detailSchemaJson,
            IsTraced = isTraced,
            TraceStage = isTraced ? Guard.Required(traceStage, nameof(traceStage), 50) : null,
            IsActive = true,
            CreatedAt = now,
        };

    public void Deactivate() => IsActive = false;
}
