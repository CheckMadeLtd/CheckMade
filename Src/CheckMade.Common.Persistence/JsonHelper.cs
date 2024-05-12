using Newtonsoft.Json;

namespace CheckMade.Common.Persistence;

public static class JsonHelper
{
    public static string SerializeToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T? DeserializeFromJsonStrict<T>(string json)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            // Throws exception during deserialization when json data has a field that doesn't map to my model class
            MissingMemberHandling = MissingMemberHandling.Error
        };
        
        return JsonConvert.DeserializeObject<T>(json, jsonSettings);
    }
}
