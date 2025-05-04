using CheckMade.Common.Model.Utils;
using Newtonsoft.Json;

namespace CheckMade.Common.Persistence.JsonHelpers;

internal static class JsonHelper
{
    public static string SerializeToJson(object obj, IDomainGlossary glossary)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new OptionContractResolver(glossary),
            Converters = new List<JsonConverter>
            {
                new DomainTermJsonConverter(glossary)
            }
        };
        
        return JsonConvert.SerializeObject(obj, jsonSettings);
    }

    public static T DeserializeFromJson<T>(string json, IDomainGlossary glossary)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            // Throws exception during deserialization when json data has a field that doesn't map to my model class
            // But does NOT throw an exception when json data LACKS a field that the model expects (instead, uses default)
            MissingMemberHandling = MissingMemberHandling.Error,
            
            ContractResolver = new OptionContractResolver(glossary),
            Converters = new List<JsonConverter>
            {
                new DomainTermJsonConverter(glossary)
            }
        };
        
        var result = JsonConvert.DeserializeObject<T>(json, jsonSettings);
        
        if (result is null)
            throw new JsonSerializationException($"Deserialization resulted in null for type {typeof(T).Name}");

        return ValidateNoNullProperties(result);
    }
    
    /// <summary>
    /// Validates that any Null value in any json details in the DB is an explicit DBNull, in which case our custom
    /// deserialization correctly translates it to an Option.None.
    /// This prevents a documented pitfall:
    /// https://github.com/CheckMadeOrga/CheckMade/wiki/Dev-Style-Guide-And-Pitfalls#serializer-assigns-default-values
    /// </summary>
    private static T ValidateNoNullProperties<T>(T obj)
    {
        var nullProperties = typeof(T).GetProperties()
            .Where(p => p.GetValue(obj) == null)
            .Select(static p => p.Name)
            .ToList();

        if (nullProperties.Count != 0)
        {
            throw new InvalidDataException(
                $"The following non-nullable properties in {typeof(T).Name} are null: " +
                $"{string.Join(", ", nullProperties)}");
        }

        return obj;
    }
}
