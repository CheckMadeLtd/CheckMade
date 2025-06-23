using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CheckMade.Services.Persistence.JsonHelpers;

/// <summary>
/// Custom contract resolver that provides specialized JSON conversion for domain-specific objects.
/// Automatically assigns appropriate converters based on the object type being serialized or deserialized.
/// </summary>
internal sealed class JsonContractResolver(IDomainGlossary glossary) : DefaultContractResolver
{
    protected override JsonContract CreateContract(Type objectType)
    {
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>))
        {
            // Get the underlying type T (e.g. 'string' in Option<string>)
            var underlyingType = objectType.GetGenericArguments().First();
            // Creates a standard contract for the objectType 
            var contract = CreateObjectContract(objectType);
            // Becomes e.g. typeof(CustomJsonConverter<string>) when underlyingType = 'string' 
            var converterType = typeof(CustomJsonConverter<>).MakeGenericType(underlyingType);
            
            contract.Converter = (JsonConverter)Activator.CreateInstance(converterType, glossary)!;

            return contract;
        }

        return base.CreateContract(objectType);
    }
}
