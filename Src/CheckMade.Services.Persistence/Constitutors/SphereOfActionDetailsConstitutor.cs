using System.Collections.Concurrent;
using System.Data.Common;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Services.Persistence.JsonHelpers;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Services.Persistence.Constitutors;

public sealed class SphereOfActionDetailsConstitutor
{
    private readonly ConcurrentDictionary<string, ISphereOfActionDetails> _detailsBySphereNameCache = new();
    
    internal Option<ISphereOfAction> ConstituteSphereOfAction(DbDataReader reader, IDomainGlossary glossary)
    {
        if (reader.IsDBNull(reader.GetOrdinal("sphere_name")))
            return Option<ISphereOfAction>.None();

        var trade = GetTrade();

        const string invalidTradeTypeException = $"""
                                                  This is not an existing '{nameof(trade)}' or we forgot to
                                                  implement a new type in method '{nameof(ConstituteSphereOfAction)}' 
                                                  """;

        var sphereName = reader.GetString(reader.GetOrdinal("sphere_name"));
        ISphereOfActionDetails? details;

        if (!_detailsBySphereNameCache.TryGetValue(sphereName, out _))
        {
            var detailsJson = reader.GetString(reader.GetOrdinal("sphere_details"));
        
            details = trade switch
            {
                SanitaryTrade => 
                    JsonHelper.DeserializeFromJson<SanitaryCampDetails>(detailsJson, glossary)
                    ?? throw new InvalidDataException($"Failed to deserialize '{nameof(SanitaryCampDetails)}'!"),
                SiteCleanTrade => 
                    JsonHelper.DeserializeFromJson<SiteCleaningZoneDetails>(detailsJson, glossary)
                    ?? throw new InvalidDataException($"Failed to deserialize '{nameof(SiteCleaningZoneDetails)}'!"),
                _ => 
                    throw new InvalidOperationException(invalidTradeTypeException)
            };

            _detailsBySphereNameCache.TryAdd(sphereName, details);
        }

        if (!_detailsBySphereNameCache.TryGetValue(sphereName, out details))
            throw new InvalidDataException($"Failed to add {nameof(ISphereOfActionDetails)} to cache.");
        
        ISphereOfAction sphere = trade switch
        {
            SanitaryTrade => 
                new SphereOfAction<SanitaryTrade>(sphereName, details),
            SiteCleanTrade => 
                new SphereOfAction<SiteCleanTrade>(sphereName, details),
            _ => 
                throw new InvalidOperationException(invalidTradeTypeException)
        };
        
        return Option<ISphereOfAction>.Some(sphere);

        ITrade GetTrade()
        {
            var tradeId = new CallbackId(reader.GetString(reader.GetOrdinal("sphere_trade")));
            var tradeType = glossary.TermById[tradeId].TypeValue;

            if (tradeType is null || 
                !tradeType.IsAssignableTo(typeof(ITrade)))
            {
                throw new InvalidDataException($"The '{nameof(tradeType)}': '{tradeType?.FullName}' of this sphere " +
                                               $"can't be determined.");
            }

            return (ITrade)Activator.CreateInstance(tradeType)!;
        }
    }
}