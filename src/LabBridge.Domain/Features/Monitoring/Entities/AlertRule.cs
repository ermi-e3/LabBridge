using LabBridge.Domain.Common;
using LabBridge.Domain.Features.Monitoring.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>Alert rule definition (platform reference data seeded by migration; never deleted, only deactivated).</summary>
public sealed class AlertRule : Entity
{
    private AlertRule() { }

    public string RuleCode { get; private set; } = null!;
    public AlertScope Scope { get; private set; }
    public RuleKind Kind { get; private set; }
    public string Description { get; private set; } = null!;
    public AlertSeverity DefaultSeverity { get; private set; }
    public string? ParameterSchemaJson { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static AlertRule Define(
        string ruleCode,
        AlertScope scope,
        RuleKind kind,
        string description,
        AlertSeverity defaultSeverity,
        string? parameterSchemaJson,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            RuleCode = Guard.Required(ruleCode, nameof(ruleCode), 100),
            Scope = Guard.Defined(scope, nameof(scope)),
            Kind = Guard.Defined(kind, nameof(kind)),
            Description = Guard.Required(description, nameof(description), 500),
            DefaultSeverity = Guard.Defined(defaultSeverity, nameof(defaultSeverity)),
            //ParameterSchemaJson = parameterSchemaJson,
            ParameterSchemaJson = Guard.OptionalJson(parameterSchemaJson, nameof(parameterSchemaJson)),
            IsActive = true,
            CreatedAt = now,
        };

    public void Deactivate() => IsActive = false;
}
