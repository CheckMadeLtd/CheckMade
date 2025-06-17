using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using Newtonsoft.Json;

namespace CheckMade.Common.Persistence.JsonHelpers;

internal sealed class DomainTermJsonConverter(IDomainGlossary glossary) : JsonConverter<DomainTerm>
{
    public override void WriteJson(JsonWriter writer, DomainTerm? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            var callbackId = glossary.IdAndUiByTerm[value].callbackId;
            writer.WriteValue(callbackId);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override DomainTerm? ReadJson(
        JsonReader reader, Type objectType, DomainTerm? existingValue, 
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var callbackId = reader.Value as string;

        return callbackId is null 
            ? null 
            : glossary.TermById[new CallbackId(callbackId)];
    }
}