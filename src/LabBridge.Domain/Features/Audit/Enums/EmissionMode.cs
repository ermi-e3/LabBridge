namespace LabBridge.Domain.Features.Audit.Enums;

/// <summary>How an action is audited: Transactional = in the business transaction (fail closed); Buffered = spooled, best effort. ASSUMPTION A-AUD-01: stored in AuditActionDefinition.failure_mode (DEC-018 Proposed).</summary>
public enum EmissionMode
{
    Transactional = 0,
    Buffered = 1,
}
