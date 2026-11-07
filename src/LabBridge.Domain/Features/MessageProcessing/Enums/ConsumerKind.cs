namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Downstream consumer of a normalized message.</summary>
public enum ConsumerKind
{
    Workflow = 0,
    Delivery = 1,
}
