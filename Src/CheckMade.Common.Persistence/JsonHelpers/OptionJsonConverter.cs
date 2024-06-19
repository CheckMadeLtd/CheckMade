using CheckMade.Common.Model.Utils;
using Newtonsoft.Json;

namespace CheckMade.Common.Persistence.JsonHelpers
{
    internal class OptionJsonConverter<T>(DomainGlossary glossary) : JsonConverter<Option<T>>
    {
        public override void WriteJson(JsonWriter writer, Option<T>? value, JsonSerializer serializer)
        {
            if (value is { IsSome: true })
            {
                if (typeof(T) == typeof(DomainTerm))
                {
                    var domainTerm = value.GetValueOrThrow() as DomainTerm;
                    var callbackId = glossary.IdAndUiByTerm.First(kvp => 
                        kvp.Key == domainTerm).Value.callbackId;
                    
                    writer.WriteValue(callbackId);
                }
                else
                {
                    serializer.Serialize(writer, value.GetValueOrThrow());
                }
            }
            else
            {
                serializer.Serialize(writer, null);
            }
        }

        public override Option<T> ReadJson(JsonReader reader, Type objectType, Option<T>? existingValue, 
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Option<T>.None();
            }

            if (typeof(T) == typeof(DomainTerm))
            {
                var callbackId = reader.Value as string;
                var domainTerm = glossary.TermById.First(t => 
                    t.Key == callbackId).Value;
                
                return Option<T>.Some((T)(object)domainTerm);
            }
            
            var value = serializer.Deserialize<T>(reader);
            return value != null 
                ? Option<T>.Some(value) 
                : Option<T>.None();
        }
    }
}