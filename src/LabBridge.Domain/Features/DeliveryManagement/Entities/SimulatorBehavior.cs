using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;
using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>Scripted behaviour of a simulated outbound connector (key = connector id). RemainingCount applies the mode to the next N sends.</summary>
public sealed class SimulatorBehavior : AggregateRoot, IOrganizationOwned
{
    private SimulatorBehavior() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId => Id;
    public SimulatorMode Mode { get; private set; }
    public int RemainingCount { get; private set; }
    public int DelaySeconds { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static SimulatorBehavior Script(
        Connector connector,
        bool simulatorEnabled,
        SimulatorMode mode,
        int remainingCount,
        int delaySeconds,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(connector);
        EnsureEligible(connector, simulatorEnabled);
        var behavior = new SimulatorBehavior
        {
            Id = connector.Id,
            OrganizationId = connector.OrganizationId,
        };
        behavior.Apply(mode, remainingCount, delaySeconds, now, actorId);
        return behavior;
    }

    public void Reconfigure(
        Connector connector,
        bool simulatorEnabled,
        SimulatorMode mode,
        int remainingCount,
        int delaySeconds,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        if (connector.Id != Id)
            throw new ValidationException("SIMULATOR_CONNECTOR_MISMATCH", "Connector mismatch",
                "The behavior belongs to a different connector.");

        EnsureEligible(connector, simulatorEnabled);
        Apply(mode, remainingCount, delaySeconds, now, actorId);
    }

    /// <summary>Consumes one scripted response; returns the mode to apply (falls back to PositiveAck when the script is exhausted).</summary>
    public SimulatorMode ConsumeNext()
    {
        if (RemainingCount <= 0)
            return SimulatorMode.PositiveAck;
        RemainingCount--;
        return Mode;
    }

    private static void EnsureEligible(Connector connector, bool simulatorEnabled)
    {
        if (
            !simulatorEnabled
            || !connector.IsSimulated
            || !connector.CanSend
            || connector.Status != ConnectorStatus.Enabled
        )
            throw new ValidationException(
                "SIMULATOR_INVALID",
                "Invalid simulator configuration",
                "Scripting requires an enabled simulated outbound connector and the simulator to be enabled."
            );
    }

    //private void Apply(
    //    SimulatorMode mode,
    //    int remaining,
    //    int delaySeconds,
    //    DateTimeOffset now,
    //    Guid? actorId
    //)
    //{
    //    Mode = Guard.Defined(mode, nameof(mode));
    //    RemainingCount =
    //        remaining >= 1
    //            ? remaining
    //            : throw new ValidationException(
    //                "SIMULATOR_INVALID",
    //                "Invalid simulator configuration",
    //                "The count must be at least 1."
    //            );
    //    DelaySeconds = Guard.InRange(delaySeconds, 0, 3600, nameof(delaySeconds));
    //    UpdatedAt = now;
    //    UpdatedBy = actorId;
    //}

    private void Apply(SimulatorMode mode, int remaining, int delaySeconds, DateTimeOffset now, Guid? actorId)
    {
        var validMode = Guard.Defined(mode, nameof(mode));
        if (remaining < 1)
            throw new ValidationException("SIMULATOR_INVALID", "Invalid simulator configuration", "The count must be at least 1.");
        var validDelay = Guard.InRange(delaySeconds, 0, 3600, nameof(delaySeconds));

        Mode = validMode;
        RemainingCount = remaining;
        DelaySeconds = validDelay;
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}
