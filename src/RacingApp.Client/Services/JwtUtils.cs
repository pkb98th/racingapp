using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RacingApp.Client.Services;

public static class JwtUtils
{
    public static IEnumerable<Claim> ParseClaims(string token)
    {
        var payload = DecodePayload(token);
        using var doc = JsonDocument.Parse(payload);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    var value = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                    if (value is not null)
                        yield return new Claim(property.Name, value);
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                yield return new Claim(property.Name, property.Value.GetString() ?? string.Empty);
            }
        }
    }

    public static bool IsExpired(string token)
    {
        try
        {
            using var doc = JsonDocument.Parse(DecodePayload(token));
            if (!doc.RootElement.TryGetProperty("exp", out var exp))
                return false;

            var seconds = exp.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private static string DecodePayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
            throw new ArgumentException("Invalid JWT token.");

        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }
}
