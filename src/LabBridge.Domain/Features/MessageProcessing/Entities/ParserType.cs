using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Enums;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>Registered parser capability (platform reference data). Parser implementations are out of scope; (Code, Version) is unique.</summary>
public sealed class ParserType : Entity
{
    private readonly List<string> _supportedMessageKindNames = [];

    private ParserType() { }

    public string Code { get; private set; } = null!;
    public string Version { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string InputFormat { get; private set; } = null!;
    public string? LimitsJson { get; private set; }
    public int CanonicalSchemaVersion { get; private set; }
    public bool IsSimulator { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<MessageKind> SupportedMessageKinds =>
        _supportedMessageKindNames.Select(Enum.Parse<MessageKind>).ToList().AsReadOnly();

    public static ParserType Register(
        string code,
        string version,
        string displayName,
        string inputFormat,
        IEnumerable<MessageKind> supportedKinds,
        string? limitsJson,
        int canonicalSchemaVersion,
        bool isSimulator,
        DateTimeOffset now
    )
    {
        var kinds = supportedKinds.Distinct().ToList();
        if (kinds.Count == 0)
            throw new ValidationException(
                "PARSER_KINDS_REQUIRED",
                "Supported kinds required",
                "A parser type must support at least one message kind."
            );
        var parser = new ParserType
        {
            Id = Guid.NewGuid(),
            Code = Guard.Required(code, nameof(code), 100),
            Version = Guard.Required(version, nameof(version), 30),
            DisplayName = Guard.Required(displayName, nameof(displayName), 200),
            InputFormat = Guard.Required(inputFormat, nameof(inputFormat), 50),
            LimitsJson = limitsJson,
            CanonicalSchemaVersion = Guard.InRange(
                canonicalSchemaVersion,
                1,
                int.MaxValue,
                nameof(canonicalSchemaVersion)
            ),
            IsSimulator = isSimulator,
            IsActive = true,
            CreatedAt = now,
        };
        parser._supportedMessageKindNames.AddRange(
            kinds.Select(k => Guard.Defined(k, nameof(supportedKinds)).ToString())
        );
        return parser;
    }

    public bool Supports(MessageKind kind) => _supportedMessageKindNames.Contains(kind.ToString());

    public void Deactivate() => IsActive = false;
}
