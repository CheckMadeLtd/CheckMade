using Newtonsoft.Json;

namespace CheckMade.Common.Persistence;

public static class JsonHelper
{
    public static string SerializeToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T? DeserializeFromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
