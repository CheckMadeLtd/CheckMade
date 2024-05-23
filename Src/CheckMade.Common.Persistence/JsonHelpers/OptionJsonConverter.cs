using CheckMade.Common.LangExt.MonadicWrappers;
using Newtonsoft.Json;

namespace CheckMade.Common.Persistence.JsonHelpers;

internal class OptionJsonConverter<T> : JsonConverter<Option<T>>
{
    public override void WriteJson(JsonWriter writer, Option<T>? value, JsonSerializer serializer)
    {
        if (value is { IsSome: true })
        {
            serializer.Serialize(writer, value.GetValueOrDefault());
        }
        else
        {
            serializer.Serialize(writer, null);
        }
    }

    public override Option<T> ReadJson(JsonReader reader, Type objectType, Option<T>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        // This makes sure that a 'null' value in the database gets converted to Option<T>.None() for any T
        if (reader.TokenType == JsonToken.Null)
        {
            return Option<T>.None();
        }
    
        var value = serializer.Deserialize<T>(reader);
        return value != null ? Option<T>.Some(value) : Option<T>.None();
    }
}