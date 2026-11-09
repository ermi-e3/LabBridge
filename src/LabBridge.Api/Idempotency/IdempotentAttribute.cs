namespace LabBridge.Api.Idempotency;


[AttributeUsage(
    AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true)]
public sealed class IdempotentAttribute(string operationScope) : Attribute
{
    public string OperationScope { get; } = operationScope;

    /// <summary>
    /// True for tenant endpoints.
    /// False for explicitly platform-scoped endpoints.
    /// </summary>
    public bool OrganizationScoped { get; set; } = true;
}