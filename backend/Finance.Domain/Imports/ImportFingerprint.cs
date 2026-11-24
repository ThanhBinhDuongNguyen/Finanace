using System.Security.Cryptography;
using System.Text;

namespace Finance.Domain.Imports;

public static class ImportFingerprint
{
    public static string Create(ImportSourceType sourceType, string shopCode, DateOnly period, string currency, IEnumerable<string> rawRows)
    {
        var payload = string.Join("\n", new[] { sourceType.ToString(), shopCode.Trim().ToUpperInvariant(), period.ToString("yyyy-MM-dd"), currency.Trim().ToUpperInvariant() }
            .Concat(rawRows.OrderBy(row => row, StringComparer.Ordinal)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
