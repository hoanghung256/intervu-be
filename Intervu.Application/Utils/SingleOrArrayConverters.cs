using System;
using System.Collections.Generic;

namespace Intervu.Application.Utils
{
    public class SingleOrArrayNewtonsoftConverter<T> : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<T>);
        }

        public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
            {
                return new List<T>();
            }

            if (reader.TokenType == Newtonsoft.Json.JsonToken.StartArray)
            {
                return serializer.Deserialize<List<T>>(reader) ?? new List<T>();
            }

            var item = serializer.Deserialize<T>(reader);
            return item == null ? new List<T>() : new List<T> { item };
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class SingleOrArraySystemTextJsonConverter<T> : System.Text.Json.Serialization.JsonConverter<List<T>>
    {
        public override List<T> Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            {
                return new List<T>();
            }

            if (reader.TokenType == System.Text.Json.JsonTokenType.StartArray)
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
            }

            var item = System.Text.Json.JsonSerializer.Deserialize<T>(ref reader, options);
            return item == null ? new List<T>() : new List<T> { item };
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, List<T> value, System.Text.Json.JsonSerializerOptions options)
        {
            System.Text.Json.JsonSerializer.Serialize(writer, value, options);
        }
    }
}
