using LabBridge.Domain.Common.Events;

namespace LabBridge.Domain.Features.InstrumentManagement;

/// <summary>Raised when a connector becomes Enabled; the runtime (outside the domain) starts the listener or dispatcher.</summary>
public sealed record ConnectorEnabled(Guid OrganizationId, Guid ConnectorId, int ConfigVersion, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

/// <summary>Raised when a connector is Disabled; the runtime stops the listener or dispatcher.</summary>
public sealed record ConnectorDisabled(Guid OrganizationId, Guid ConnectorId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
