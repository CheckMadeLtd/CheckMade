using CheckMade.Core.ServiceInterfaces.Bot;
using Newtonsoft.Json;

namespace CheckMade.Services.Persistence.JsonHelpers;

public static class JsonHelper
{
    public static string SerializeToJson(object obj, IDomainGlossary glossary)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new JsonContractResolver(glossary),
            Converters = new List<JsonConverter>
            {
                new DomainTermJsonConverter(glossary)
            }
        };
        
        return JsonConvert.SerializeObject(obj, jsonSettings);
    }

    /// <summary>
    /// Strict default setting for production code to force keeping historic 'details' data up to date with migrations.
    /// Only ignore missing members e.g. for the purpose of migration operations. 
    /// </summary>
    public static T DeserializeFromJson<T>(string json, IDomainGlossary glossary, bool ignoreMissingMembers = false)
    {
        var totalSw = System.Diagnostics.Stopwatch.StartNew();
    
        var settingsSw = System.Diagnostics.Stopwatch.StartNew();
        var jsonSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = ignoreMissingMembers  
                ? MissingMemberHandling.Ignore 
                : MissingMemberHandling.Error,
        
            ContractResolver = new JsonContractResolver(glossary),
            Converters = new List<JsonConverter>
            {
                new DomainTermJsonConverter(glossary)
            }
        };
        settingsSw.Stop();
    
        var deserializeSw = System.Diagnostics.Stopwatch.StartNew();
        var result = JsonConvert.DeserializeObject<T>(json, jsonSettings);
        deserializeSw.Stop();
    
        if (result is null)
            throw new JsonSerializationException($"Deserialization resulted in null for type {typeof(T).Name}");

        var validateSw = System.Diagnostics.Stopwatch.StartNew();
        var validatedResult = ValidateNoNullProperties(result);
        validateSw.Stop();
    
        totalSw.Stop();

        const int deserializeWarningThreshold = 10;
        
        if (totalSw.ElapsedMilliseconds > deserializeWarningThreshold)
        {
            Console.WriteLine($"[PERF-DEBUG] for {nameof(DeserializeFromJson)} " +
                              $"(threshold: {deserializeWarningThreshold}) " +
                              $"Settings: {settingsSw.ElapsedMilliseconds}ms, " +
                              $"Deserialize: {deserializeSw.ElapsedMilliseconds}ms, " +
                              $"Validate: {validateSw.ElapsedMilliseconds}ms, " +
                              $"Total: {totalSw.ElapsedMilliseconds}ms, " +
                              $"JsonLength: {json.Length}");
        }
    
        return validatedResult;
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
    }}
