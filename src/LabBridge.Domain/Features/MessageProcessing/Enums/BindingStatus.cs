namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Profile parser binding status; at most one Active per profile version.</summary>
public enum BindingStatus
{
    Active = 0,
    Retired = 1,
}
