using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.GIS;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckMade.Services.Persistence.JsonHelpers;

internal sealed class OptionJsonConverter<T>(IDomainGlossary glossary) : JsonConverter<Option<T>>
{
    public override void WriteJson(JsonWriter writer, Option<T>? value, JsonSerializer serializer)
    {
        if (value is { IsSome: true })
        {
            if (typeof(T) == typeof(DomainTerm))
            {
                var domainTerm = value.GetValueOrThrow() as DomainTerm;
                var callbackId = glossary.GetId(domainTerm!);
                    
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
            return Option<T>.None();

        if (typeof(T) == typeof(DomainTerm))
            return ReconstructDomainTerm(reader);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (typeof(T) == typeof(Geo))
            return ReconstructGeoLocation(reader);

        return ReconstructSimpleType(serializer, reader);
    }

    private Option<T> ReconstructDomainTerm(JsonReader reader)
    {
        var callbackId = reader.Value as string;
        var domainTerm = glossary.TermById
            .First(t => t.Key == callbackId).Value;
            
        return Option<T>.Some((T)(object)domainTerm);
    }

    private static Option<T> ReconstructGeoLocation(JsonReader reader)
    {
        var geoObj = JObject.Load(reader);

        var latitudeRaw = geoObj[nameof(Latitude)]?[nameof(Latitude.Value)]?.Value<double>() 
                          ?? throw new InvalidOperationException(
                              $"Failed to read {nameof(Latitude)} from Json during deserialization attempt.");

        var longitudeRaw = geoObj[nameof(Longitude)]?[nameof(Longitude.Value)]?.Value<double>() 
                           ?? throw new InvalidOperationException(
                               $"Failed to read {nameof(Longitude)} from Json during deserialization attempt.");
                
        var uncertaintyRadiusRaw = geoObj[nameof(Geo.UncertaintyRadiusInMeters)]?.Value<float?>();
                
        var uncertaintyRadius = uncertaintyRadiusRaw != null
            ? Option<double>.Some(uncertaintyRadiusRaw.Value)
            : Option<double>.None(); 

        var latitudeSafe = new Latitude(latitudeRaw);
        var longitudeSafe = new Longitude(longitudeRaw);
        var geoLocation = new Geo(latitudeSafe, longitudeSafe, uncertaintyRadius);

        return Option<T>.Some((T)(object)geoLocation);
    }

    private static Option<T> ReconstructSimpleType(JsonSerializer serializer, JsonReader reader)
    {
        var value = serializer.Deserialize<T>(reader);
            
        return value != null 
            ? Option<T>.Some(value) 
            : Option<T>.None();
    }
}