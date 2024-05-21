using System.Text.Json;

namespace Shared.Utils;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}