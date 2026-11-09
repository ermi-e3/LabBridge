using System.Security.Cryptography;
using System.Text;

namespace LabBridge.Api.Idempotency;

internal static class RequestFingerprint
{
    public static string Create(
        string method,
        string path,
        string queryString,
        string? contentType,
        byte[] requestBody)
    {
        var metadata = string.Join(
            "\n",
            method.ToUpperInvariant(),
            path,
            queryString,
            contentType ?? string.Empty);

        var metadataBytes = Encoding.UTF8.GetBytes(metadata);

        using var hash = IncrementalHash.CreateHash(
            HashAlgorithmName.SHA256);

        hash.AppendData(metadataBytes);
        hash.AppendData([0]);
        hash.AppendData(requestBody);

        return Convert.ToHexString(hash.GetHashAndReset());
    }
}