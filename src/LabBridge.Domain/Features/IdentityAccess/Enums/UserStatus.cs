namespace LabBridge.Domain.Features.IdentityAccess.Enums;

/// <summary>User lifecycle status (Stage 1 §8, adopted unchanged by Module 1).</summary>
public enum UserStatus
{
    Invited = 0,
    Active = 1,
    Locked = 2,
    Disabled = 3,
}
