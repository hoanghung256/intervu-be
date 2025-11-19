using System.Text.Json;

namespace Intervu.API.Utils
{
    public class JsonElementComparer : IEqualityComparer<JsonElement>
    {
        public bool Equals(JsonElement x, JsonElement y)
        {
            if (x.ValueKind != y.ValueKind)
                return false;

            switch (x.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Undefined:
                    return true;

                case JsonValueKind.Number:
                    // Use a small tolerance for floating-point comparison
                    const double epsilon = 1e-5;
                    return Math.Abs(x.GetDouble() - y.GetDouble()) < epsilon;

                case JsonValueKind.String:
                    return x.GetString() == y.GetString();

                case JsonValueKind.Object:
                    var xProperties = x.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                    var yProperties = y.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                    if (xProperties.Count != yProperties.Count)
                        return false;

                    foreach (var property in xProperties)
                    {
                        if (!yProperties.TryGetValue(property.Key, out var yValue) || !Equals(property.Value, yValue))
                            return false;
                    }
                    return true;

                case JsonValueKind.Array:
                    if (x.GetArrayLength() != y.GetArrayLength())
                        return false;

                    // For arrays, order matters.
                    for (int i = 0; i < x.GetArrayLength(); i++)
                    {
                        if (!Equals(x[i], y[i]))
                            return false;
                    }
                    return true;

                default:
                    throw new JsonException($"Unsupported JsonValueKind: {x.ValueKind}");
            }
        }

        public int GetHashCode(JsonElement obj)
        {
            // This is sufficient for our use case as we are not using it in a hash-based collection.
            return obj.GetRawText().GetHashCode();
        }
    }
}
