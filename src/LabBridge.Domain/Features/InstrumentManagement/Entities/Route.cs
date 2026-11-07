using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Enums;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// Directed path for one message kind between a source connector and a destination connector for an instrument (BR-CON-003).
/// Order route: order-source system connector (receives) -> instrument connector (sends).
/// Result route: instrument connector (receives) -> result-destination system connector (sends).
/// Created Disabled; never deleted in the MVP (BR-CON-014). At most one Enabled route per (organization, source connector, kind) (BR-CON-009).
/// </summary>
public sealed class Route : AuditedAggregateRoot, IOrganizationOwned
{
    private Route() { }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public MessageKind MessageKind { get; private set; }
    public Guid SourceConnectorId { get; private set; }
    public Guid DestinationConnectorId { get; private set; }
    public Guid InstrumentId { get; private set; }
    public InstrumentTopology Topology { get; private set; }
    public RouteStatus Status { get; private set; }
    public bool IsSimulated { get; private set; }

    public static Route Define(
        string name,
        MessageKind messageKind,
        Connector source,
        Connector destination,
        Instrument instrument,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(instrument);
        if (messageKind is not (MessageKind.Order or MessageKind.Result))
            throw new ValidationException(
                "ROUTE_KIND_INVALID",
                "Invalid route kind",
                "A route carries Order or Result messages."
            );
        if (
            source.OrganizationId != instrument.OrganizationId
            || destination.OrganizationId != instrument.OrganizationId
        )
            throw new ForbiddenException(
                "TENANT_ACCESS_DENIED",
                "Tenant access denied",
                "Route endpoints must belong to the organization of the instrument."
            );
        if (source.Id == destination.Id)
            throw new ValidationException(
                "ROUTE_ENDPOINTS_IDENTICAL",
                "Invalid route endpoints",
                "Source and destination connectors must differ."
            );
        if (instrument.Topology is null)
            throw new BusinessRuleException(
                "ROUTE_TOPOLOGY_UNSET",
                "Topology not set",
                "The instrument must define its order-source and result-destination systems before routes are defined."
            );
        if (!source.CanReceive || !destination.CanSend)
            throw new ValidationException(
                "ROUTE_DIRECTION_INVALID",
                "Invalid connector directions",
                "The source must be able to receive and the destination able to send."
            );

        var shapeValid =
            messageKind == MessageKind.Order
                ? source.ExternalSystemId is { } s
                    && s == instrument.OrderSourceSystemId
                    && destination.InstrumentId == instrument.Id
                : source.InstrumentId == instrument.Id
                    && destination.ExternalSystemId is { } d
                    && d == instrument.ResultDestinationSystemId;
        if (!shapeValid)
            throw new ValidationException(
                "ROUTE_SHAPE_INVALID",
                "Invalid route shape",
                "The route endpoints do not match the instrument's configured systems."
            );

        var route = new Route
        {
            Id = Guid.NewGuid(),
            OrganizationId = instrument.OrganizationId,
            Name = Guard.Required(name, nameof(name), 200),
            MessageKind = messageKind,
            SourceConnectorId = source.Id,
            DestinationConnectorId = destination.Id,
            InstrumentId = instrument.Id,
            Topology = instrument.Topology.Value,
            Status = RouteStatus.Disabled,
            IsSimulated = source.IsSimulated || destination.IsSimulated,
        };
        route.StampCreated(now, actorId);
        return route;
    }

    public void Rename(string name, DateTimeOffset now, Guid? actorId)
    {
        Name = Guard.Required(name, nameof(name), 200);
        StampUpdated(now, actorId);
    }

    /// <param name="conflictingEnabledRouteExists">True when another route is Enabled for the same (organization, source connector, kind).</param>
    public void Enable(bool conflictingEnabledRouteExists, DateTimeOffset now, Guid? actorId)
    {
        if (Status == RouteStatus.Enabled)
            return;
        if (conflictingEnabledRouteExists)
            throw new ConflictException(
                "ROUTE_CONFLICT",
                "Route conflict",
                "Another route is already enabled for this source connector and message kind."
            );
        Status = RouteStatus.Enabled;
        StampUpdated(now, actorId);
    }

    public void Disable(DateTimeOffset now, Guid? actorId)
    {
        if (Status == RouteStatus.Disabled)
            return;
        Status = RouteStatus.Disabled;
        StampUpdated(now, actorId);
    }
}
