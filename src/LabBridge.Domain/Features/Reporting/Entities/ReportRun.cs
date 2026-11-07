using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Reporting.Enums;

namespace LabBridge.Domain.Features.Reporting.Entities;

/// <summary>A report execution. The result holds aggregates only (Confidential). RequestKey (optional) makes repeated requests idempotent per organization.</summary>
public sealed class ReportRun : AggregateRoot, IOrganizationOwned
{
    private ReportRun() { }

    public Guid OrganizationId { get; private set; }
    public string ReportCode { get; private set; } = null!;
    public string ParametersJson { get; private set; } = null!;
    public ReportRunStatus Status { get; private set; }
    public bool IncludesSimulated { get; private set; }
    public bool Partial { get; private set; }
    public string? ResultJson { get; private set; }
    public string? RequestKey { get; private set; }
    public Guid RequestedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public static ReportRun Start(
        Guid organizationId,
        ReportDefinition report,
        string parametersJson,
        bool includesSimulated,
        string? requestKey,
        Guid requestedBy,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(report);
        if (!report.IsActive)
            throw new BusinessRuleException(
                "REPORT_INACTIVE",
                "Report inactive",
                "The report definition is not active."
            );
        return new ReportRun
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            ReportCode = report.ReportCode,
            //ParametersJson = Guard.RequiredExact(
            //    parametersJson,
            //    nameof(parametersJson),
            //    int.MaxValue
            //),
            ParametersJson = Guard.RequiredJson(parametersJson, nameof(parametersJson)),
            Status = ReportRunStatus.Running,
            IncludesSimulated = includesSimulated,
            RequestKey = Guard.Optional(requestKey, nameof(requestKey), 100),
            RequestedBy = Guard.NotEmpty(requestedBy, nameof(requestedBy)),
            CreatedAt = now,
        };
    }

    public void Succeed(
        string resultJson,
        bool partial,
        DateTimeOffset expiresAt,
        DateTimeOffset now
    )
    {
        Require(ReportRunStatus.Running);
        if (expiresAt <= now)
            throw new ValidationException(
                "REPORT_EXPIRY_INVALID",
                "Invalid expiry",
                "The result expiry must be in the future."
            );
        //ResultJson = Guard.RequiredExact(resultJson, nameof(resultJson), int.MaxValue);
        ResultJson = Guard.RequiredJson(resultJson, nameof(resultJson));
        Partial = partial;
        ExpiresAt = expiresAt;
        CompletedAt = now;
        Status = ReportRunStatus.Succeeded;
    }

    public void Fail(DateTimeOffset now)
    {
        Require(ReportRunStatus.Running);
        CompletedAt = now;
        Status = ReportRunStatus.Failed;
    }

    /// <summary>Clears the result after the retention period; the run record stays.</summary>
    public void Expire()
    {
        if (Status == ReportRunStatus.Expired)
            return;
        if (Status == ReportRunStatus.Running)
            throw new BusinessRuleException(
                "REPORT_RUN_INVALID_STATE",
                "Invalid run state",
                "A running report cannot expire."
            );
        ResultJson = null;
        Status = ReportRunStatus.Expired;
    }

    private void Require(ReportRunStatus expected)
    {
        if (Status != expected)
            throw new BusinessRuleException(
                "REPORT_RUN_INVALID_STATE",
                "Invalid run state",
                $"The report run must be {expected}."
            );
    }
}
