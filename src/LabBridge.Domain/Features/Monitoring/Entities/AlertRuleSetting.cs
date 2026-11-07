using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Monitoring.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>Organization override of an organization-scope alert rule (enabled flag, severity, thresholds). Unique per (organization, rule).</summary>
public sealed class AlertRuleSetting : AggregateRoot, IOrganizationOwned
{
    private AlertRuleSetting() { }

    public Guid OrganizationId { get; private set; }
    public Guid AlertRuleId { get; private set; }
    public bool Enabled { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string? ThresholdsJson { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static AlertRuleSetting Create(
        Guid organizationId,
        AlertRule rule,
        bool enabled,
        AlertSeverity severity,
        string? thresholdsJson,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (rule.Scope != AlertScope.Organization || !rule.IsActive)
            throw new BusinessRuleException(
                "ALERT_RULE_NOT_CONFIGURABLE",
                "Rule not configurable",
                "Only active organization-scope rules can be configured by an organization."
            );
        return new AlertRuleSetting
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            AlertRuleId = rule.Id,
            Enabled = enabled,
            Severity = Guard.Defined(severity, nameof(severity)),
            //ThresholdsJson = thresholdsJson,
            ThresholdsJson = Guard.OptionalJson(thresholdsJson, nameof(thresholdsJson)),
            UpdatedAt = now,
            UpdatedBy = actorId,
        };
    }

    public void Change(
        bool enabled,
        AlertSeverity severity,
        string? thresholdsJson,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Enabled = enabled;
        Severity = Guard.Defined(severity, nameof(severity));
        //ThresholdsJson = thresholdsJson;
        ThresholdsJson = Guard.OptionalJson(thresholdsJson, nameof(thresholdsJson));
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}
