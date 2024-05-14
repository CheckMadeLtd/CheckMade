using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CheckMade.Common.LanguageExtensions.MonadicWrappers;

namespace CheckMade.Common.Persistence.JsonHelpers;

internal class OptionContractResolver : DefaultContractResolver
{
    protected override JsonContract CreateContract(Type objectType)
    {
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>))
        {
            // Get the underlying type T (e.g. 'string' in Option<string>)
            var underlyingType = objectType.GetGenericArguments().First();
            // Creates a standard contract for the objectType 
            var contract = base.CreateObjectContract(objectType);
            // Becomes e.g. typeof(OptionJsonConverter<string>) when underlyingType = 'string' 
            var converterType = typeof(OptionJsonConverter<>).MakeGenericType(underlyingType);
            
            contract.Converter = (JsonConverter)Activator.CreateInstance(converterType)!;

            return contract;
        }

        return base.CreateContract(objectType);
    }
}
