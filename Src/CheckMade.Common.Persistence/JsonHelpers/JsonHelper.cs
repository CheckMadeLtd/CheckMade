using Newtonsoft.Json;

namespace CheckMade.Common.Persistence.JsonHelpers;

internal static class JsonHelper
{
    public static string SerializeToJson(object obj)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new OptionContractResolver()
        };
        
        return JsonConvert.SerializeObject(obj, jsonSettings);
    }

    public static T? DeserializeFromJsonStrict<T>(string json)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            // Throws exception during deserialization when json data has a field that doesn't map to my model class
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new OptionContractResolver()
        };
        
        return JsonConvert.DeserializeObject<T>(json, jsonSettings);
    }
}
