using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.ServiceInterfaces.Bot;
using Newtonsoft.Json;

namespace CheckMade.Services.Persistence.JsonHelpers;

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