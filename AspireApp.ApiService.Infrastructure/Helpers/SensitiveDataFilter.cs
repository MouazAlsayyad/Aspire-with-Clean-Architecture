using System.Text.Json;
using System.Text.RegularExpressions;

namespace AspireApp.ApiService.Infrastructure.Helpers;

/// <summary>
/// Helper class for filtering sensitive data from logs
/// </summary>
public static class SensitiveDataFilter
{
    private static readonly string[] SensitiveFieldNames = new[]
    {
        "password",
        "pwd",
        "passwd",
        "token",
        "accessToken",
        "refreshToken",
        "authorization",
        "auth",
        "secret",
        "secretKey",
        "apiKey",
        "apikey",
        "api_key",
        "privateKey",
        "private_key",
        "credential",
        "credentials",
        "ssn",
        "socialSecurityNumber",
        "creditCard",
        "creditcard",
        "cardNumber",
        "cvv",
        "cvc",
        "pin",
        "otp",
        "oneTimePassword"
    };

    private static readonly string MaskedValue = "***REDACTED***";

    /// <summary>
    /// Filters sensitive data from a JSON string
    /// </summary>
    public static string FilterJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var filtered = FilterJsonElement(document.RootElement);
            return JsonSerializer.Serialize(filtered);
        }
        catch
        {
            // If JSON parsing fails, try regex-based filtering
            return FilterString(json);
        }
    }

    /// <summary>
    /// Filters sensitive data from a string (query string, form data, etc.)
    /// </summary>
    public static string FilterString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var result = input;

        // Filter query string parameters
        foreach (var fieldName in SensitiveFieldNames)
        {
            // Pattern: fieldName=value or fieldName:value or "fieldName":"value"
            var patterns = new[]
            {
                $@"({fieldName}\s*=\s*)([^&\s""']+)",  // Query string: password=value
                $@"(""{fieldName}""\s*:\s*"")([^""]+)("")",  // JSON: "password":"value"
                $@"('{fieldName}'\s*:\s*')([^']+)(')",  // JSON: 'password':'value'
                $@"({fieldName}\s*:\s*)([^,\s}}""']+)",  // JSON: password:value
            };

            foreach (var pattern in patterns)
            {
                result = Regex.Replace(
                    result,
                    pattern,
                    match => $"{match.Groups[1].Value}{MaskedValue}{(match.Groups.Count > 3 ? match.Groups[3].Value : "")}",
                    RegexOptions.IgnoreCase);
            }
        }

        return result;
    }

    /// <summary>
    /// Filters sensitive data from a dictionary
    /// </summary>
    public static Dictionary<string, object> FilterDictionary(Dictionary<string, object> dictionary)
    {
        if (dictionary == null)
        {
            return new Dictionary<string, object>();
        }

        var filtered = new Dictionary<string, object>();

        foreach (var kvp in dictionary)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (IsSensitiveField(key))
            {
                filtered[key] = MaskedValue;
            }
            else if (value is Dictionary<string, object> nestedDict)
            {
                filtered[key] = FilterDictionary(nestedDict);
            }
            else if (value is string strValue)
            {
                filtered[key] = FilterString(strValue);
            }
            else
            {
                filtered[key] = value;
            }
        }

        return filtered;
    }

    /// <summary>
    /// Checks if a field name is considered sensitive
    /// </summary>
    public static bool IsSensitiveField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        var lowerFieldName = fieldName.ToLowerInvariant();
        return SensitiveFieldNames.Any(sensitive =>
            lowerFieldName.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Filters sensitive headers from a dictionary of headers
    /// </summary>
    public static Dictionary<string, string> FilterHeaders(Dictionary<string, string> headers)
    {
        if (headers == null)
        {
            return new Dictionary<string, string>();
        }

        var filtered = new Dictionary<string, string>();

        foreach (var kvp in headers)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (IsSensitiveField(key))
            {
                filtered[key] = MaskedValue;
            }
            else
            {
                filtered[key] = value;
            }
        }

        return filtered;
    }

    /// <summary>
    /// Recursively filters sensitive data from a JsonElement
    /// </summary>
    private static object FilterJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => FilterJsonObject(element),
            JsonValueKind.Array => FilterJsonArray(element),
            JsonValueKind.String => FilterString(element.GetString() ?? ""),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Filters sensitive data from a JSON object
    /// </summary>
    private static Dictionary<string, object> FilterJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            var key = property.Name;
            var value = property.Value;

            if (IsSensitiveField(key))
            {
                result[key] = MaskedValue;
            }
            else
            {
                result[key] = FilterJsonElement(value);
            }
        }

        return result;
    }

    /// <summary>
    /// Filters sensitive data from a JSON array
    /// </summary>
    private static List<object> FilterJsonArray(JsonElement element)
    {
        var result = new List<object>();

        foreach (var item in element.EnumerateArray())
        {
            result.Add(FilterJsonElement(item));
        }

        return result;
    }
}

