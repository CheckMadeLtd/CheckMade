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

    public static T? DeserializeFromJsonStrict<T>(string json, IDomainGlossary glossary)
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
        
        return JsonConvert.DeserializeObject<T>(json, jsonSettings);
    }
}
