namespace LabBridge.Api.Idempotency;


public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);

    public int MaxRequestBodyBytes { get; set; } = 1_048_576;

    public int MaxResponseBodyBytes { get; set; } = 1_048_576;

    public string HeaderName { get; set; } = "Idempotency-Key";
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    public int CleanupBatchSize { get; set; } = 500;
}