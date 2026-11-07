using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.MessageProcessing.Enums;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>Binds an interface profile version to a parser type (platform reference data). At most one Active binding per profile version.</summary>
public sealed class ProfileParserBinding : Entity
{
    private ProfileParserBinding() { }

    public Guid ProfileVersionId { get; private set; }
    public Guid ParserTypeId { get; private set; }

    /// <summary>Non-secret parser configuration.</summary>
    public string? ConfigJson { get; private set; }
    public BindingStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ProfileParserBinding Create(
        Guid profileVersionId,
        ParserType parserType,
        string? configJson,
        DateTimeOffset now
    )
    {
        if (!parserType.IsActive)
            throw new BusinessRuleException("PARSER_INACTIVE", "Parser inactive", "An inactive parser type cannot be bound to a profile version.");

        ArgumentNullException.ThrowIfNull(parserType);
        return new ProfileParserBinding
        {
            Id = Guid.NewGuid(),
            ProfileVersionId = Guard.NotEmpty(profileVersionId, nameof(profileVersionId)),
            ParserTypeId = parserType.Id,
            ConfigJson = configJson,
            Status = BindingStatus.Active,
            CreatedAt = now,
        };
    }

    public void Retire() => Status = BindingStatus.Retired;
}
