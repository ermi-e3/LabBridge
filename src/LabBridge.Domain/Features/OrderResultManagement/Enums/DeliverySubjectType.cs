namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Kind of record a delivery request refers to (polymorphic reference, validated by the application).</summary>
public enum DeliverySubjectType
{
    Order = 0,
    Result = 1,
}
