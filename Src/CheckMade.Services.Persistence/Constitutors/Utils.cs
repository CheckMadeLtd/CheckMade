using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.Actors.RoleTypes;
using CheckMade.Core.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;
using General.Utils.Validators;

namespace CheckMade.Services.Persistence.Constitutors;

internal static class Utils
{
    internal static Option<T> GetOption<T>(DbDataReader reader, int ordinal)
    {
        var valueRaw = reader.GetValue(ordinal);

        if (typeof(T) == typeof(EmailAddress) && valueRaw != DBNull.Value)
        {
            return (Option<T>) (object) Option<EmailAddress>.Some(
                new EmailAddress(reader.GetFieldValue<string>(ordinal)));
        }
        
        return valueRaw != DBNull.Value
            ? Option<T>.Some(reader.GetFieldValue<T>(ordinal))
            : Option<T>.None();
    }

    internal static TEnum EnsureEnumValidityOrThrow<TEnum>(TEnum uncheckedEnum) where TEnum : Enum
    {
        if (!EnumChecker.IsDefined(uncheckedEnum))
            throw new InvalidDataException($"The value {uncheckedEnum} for enum of type {typeof(TEnum)} is invalid. " + 
                                           $"Forgot to migrate data in db?");
        
        return uncheckedEnum;
    }
    
    internal static readonly Dictionary<string, Func<IRoleType>> RoleTypeFactoryByFullTypeName = new()
    {
        [typeof(LiveEventAdmin).FullName!] = static () => new LiveEventAdmin(),
        [typeof(LiveEventObserver).FullName!] = static () => new LiveEventObserver(),
            
        [typeof(TradeAdmin<SanitaryTrade>).FullName!] = () => new TradeAdmin<SanitaryTrade>(),
        [typeof(TradeInspector<SanitaryTrade>).FullName!] = () => new TradeInspector<SanitaryTrade>(),
        [typeof(TradeEngineer<SanitaryTrade>).FullName!] = () => new TradeEngineer<SanitaryTrade>(),
        [typeof(TradeTeamLead<SanitaryTrade>).FullName!] = () => new TradeTeamLead<SanitaryTrade>(),
        [typeof(TradeObserver<SanitaryTrade>).FullName!] = () => new TradeObserver<SanitaryTrade>(),
            
        [typeof(TradeAdmin<SiteCleanTrade>).FullName!] = () => new TradeAdmin<SiteCleanTrade>(),
        [typeof(TradeInspector<SiteCleanTrade>).FullName!] = () => new TradeInspector<SiteCleanTrade>(),
        [typeof(TradeEngineer<SiteCleanTrade>).FullName!] = () => new TradeEngineer<SiteCleanTrade>(),
        [typeof(TradeTeamLead<SiteCleanTrade>).FullName!] = () => new TradeTeamLead<SiteCleanTrade>(),
        [typeof(TradeObserver<SiteCleanTrade>).FullName!] = () => new TradeObserver<SiteCleanTrade>(),
    };
}