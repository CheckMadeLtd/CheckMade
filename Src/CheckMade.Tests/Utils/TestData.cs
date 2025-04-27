using System.Collections.Immutable;
using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Core.Trades.Concrete;
using User = CheckMade.Common.Model.Core.Actors.Concrete.User;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace CheckMade.Tests.Utils;

internal static class TestData
{
    #region UsersSetup #################################################################################################

    internal static readonly Vendor EveConGmbH = new Vendor("eveCon GmbH");
    
    internal static readonly User DanielEn = new(
        new MobileNumber("+447777111999"),
        "_Daniel",
        "Test English",
        "_Gorin",
        new EmailAddress("daniel-test-checkmade@neocortek.net"),
        LanguageCode.en,
        new List<IRoleInfo>(),
        Option<Vendor>.None());
    
    internal static readonly User DanielDe = new(
        new MobileNumber("+447777111888"),
        "_Daniel",
        "Test German",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.de,
        new List<IRoleInfo>(),
        Option<Vendor>.None());
    
    internal static readonly User LukasDe = new(
        new MobileNumber("+49111199999"),
        "_Lukas",
        "Test German",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.de,
        new List<IRoleInfo>(),
        EveConGmbH);
    
    #endregion
    
    #region ILiveEventInfoSetup ########################################################################################
    
    // Venues
    
    internal static readonly LiveEventVenue Venue1 = new("Venue1 near Cologne");
    internal static readonly LiveEventVenue Venue2 = new("Venue2 near Bremen");

    // 2024 ILiveEventInfos

    internal static readonly LiveEventInfo X2024Info = new("LiveEvent X 2024",
        new DateTime(2024, 07, 19, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 07, 22, 18, 00, 00, DateTimeKind.Utc));

    internal static readonly LiveEventInfo Y2024Info = new("LiveEvent Y 2024",
        new DateTime(2024, 06, 21, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 06, 24, 18, 00, 00, DateTimeKind.Utc));
    
    // 2025 ILiveEventInfos

    internal static readonly LiveEventInfo X2025Info = new("LiveEvent X 2025",
        new DateTime(2025, 07, 18, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 07, 21, 18, 00, 00, DateTimeKind.Utc));

    internal static readonly LiveEventInfo Y2025Info = new("LiveEvent Y 2025",
        new DateTime(2025, 06, 20, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 06, 23, 18, 00, 00, DateTimeKind.Utc));
    
    #endregion
    
    #region RoleSetup ##################################################################################################
    
    // Default for testing
    internal static readonly Role SanitaryAdmin_DanielEn_X2024 = 
        new("RVB70T",
            new TradeAdmin<SanitaryTrade>(), 
            new UserInfo(DanielEn),
            X2024Info);
    
    internal static readonly Role SanitaryInspector_DanielEn_X2024 = 
        new("R3UDXW",
            new TradeInspector<SanitaryTrade>(),
            new UserInfo(DanielEn),
            X2024Info);
    
    internal static readonly Role SanitaryInspector_DanielEn_X2025 = 
        new("R9AAB5",
            new TradeInspector<SanitaryTrade>(),
            new UserInfo(DanielEn),
            X2025Info);
    
    internal static readonly Role SanitaryInspector_LukasDe_X2024 = 
        new("R7UIP8",
            new TradeInspector<SanitaryTrade>(),
            new UserInfo(LukasDe),
            X2024Info);

    internal static readonly Role SanitaryTeamLead_DanielDe_X2024 = 
        new("R2JXNM",
            new TradeTeamLead<SanitaryTrade>(),
            new UserInfo(DanielDe),
            X2024Info);

    internal static readonly Role SanitaryObserver_DanielEn_X2024 = 
        new("RYEATF",
            new TradeObserver<SanitaryTrade>(),
            new UserInfo(DanielEn),
            X2024Info);
    
    internal static readonly Role SanitaryInspector_DanielDe_X2024 = 
        new("RMAM8S",
            new TradeInspector<SanitaryTrade>(),
            new UserInfo(DanielDe),
            X2024Info);
    
    internal static readonly Role SanitaryEngineer_DanielEn_X2024 = 
        new("RGR37T",
            new TradeEngineer<SanitaryTrade>(),
            new UserInfo(DanielEn),
            X2024Info);
    
    internal static readonly Role SanitaryTeamLead_DanielEn_X2024 = 
        new("RI8MJ1",
            new TradeTeamLead<SanitaryTrade>(),
            new UserInfo(DanielEn), 
            X2024Info);

    internal static readonly Role LiveEventAdmin_DanielEn_X2024 =
        new("R23QI6",
            new LiveEventAdmin(),
            new UserInfo(DanielEn),
            X2024Info);
    
    internal static readonly Role SanitaryEngineer_DanielEn_Y2024 = 
        new("RS0BSU",
            new TradeEngineer<SanitaryTrade>(),
            new UserInfo(DanielEn),
            Y2024Info);
    
    internal static readonly Role SiteCleanEngineer_DanielEn_X2024 = 
        new("RMWC16",
            new TradeEngineer<SiteCleanTrade>(),
            new UserInfo(DanielEn),
            X2024Info);
    
    #endregion

    #region LiveEventSetup #############################################################################################
    
    // 2024 LiveEvents
    
    internal static readonly Geo Location_Dassel = 
        new(50.93140957842968, 6.932814116544934, Option<double>.None());
    
    internal static readonly Geo Location_4cc =
        new Geo(51.240118, -0.789397, Option<double>.None());

    internal static readonly SphereOfAction<SanitaryTrade> Sphere1_AtX2024 =
        new("CampDassel",
            new SanitaryCampDetails(
                Location_Dassel,
                new List<DomainTerm>
                {
                    Dt(typeof(GeneralMisc)),
                    Dt(typeof(Shower)),
                    Dt(typeof(Toilet))
                }.ToImmutableArray(),
                new List<DomainTerm>
                {
                    Dt(ConsumablesItem.ToiletPaper),
                    Dt(ConsumablesItem.PaperTowels),
                    Dt(ConsumablesItem.Soap)
                }));
    
    internal static readonly SphereOfAction<SanitaryTrade> Sphere2_AtX2024 =
        new("Camp2-4cc",
            new SanitaryCampDetails(
                Location_4cc,
                new List<DomainTerm>
                    {
                        Dt(typeof(GeneralMisc))
                    }
                    .ToImmutableArray(),
                new List<DomainTerm>
                {
                    Dt(ConsumablesItem.ToiletPaper)
                }));
    
    internal static readonly SphereOfAction<SanitaryTrade> Sphere3_AtX2024 =
        new("Camp3-NoGeo",
            new SanitaryCampDetails(
                Option<Geo>.None(), 
                new List<DomainTerm>
                    {
                        Dt(typeof(GeneralMisc))
                    }
                    .ToImmutableArray(),
                new List<DomainTerm>
                {
                    Dt(ConsumablesItem.ToiletPaper)
                }));

    internal static readonly SphereOfAction<SiteCleanTrade> Sphere4_AtX2024 =
        new("Zone1",
            new SiteCleaningZoneDetails(
                Option<Geo>.None(),
                new List<DomainTerm>().ToImmutableArray(),
                new List<DomainTerm>().ToImmutableArray()));
    
    internal static readonly LiveEvent X2024 = new(
        X2024Info,
        new List<IRoleInfo>
        {
            SanitaryInspector_LukasDe_X2024,
            SanitaryAdmin_DanielEn_X2024,
            LiveEventAdmin_DanielEn_X2024,
            SanitaryEngineer_DanielEn_X2024,
            SiteCleanEngineer_DanielEn_X2024,
            SanitaryTeamLead_DanielEn_X2024
        },
        Venue1,
        new List<ISphereOfAction>
        {
            Sphere1_AtX2024, Sphere2_AtX2024, Sphere4_AtX2024
        });

    internal static readonly LiveEvent Y2024 = new(
        Y2024Info,
        new List<IRoleInfo>(),
        Venue2,
        new List<ISphereOfAction>());

    // 2025 LiveEvents
    
    internal static readonly LiveEvent X2025 = new(
        X2025Info,
        new List<IRoleInfo>
        {
            SanitaryInspector_DanielEn_X2025
        },
        Venue1,
        new List<ISphereOfAction>());

    internal static readonly LiveEvent Y2025 = new(
        Y2025Info,
        new List<IRoleInfo>(),
        Venue2,
        new List<ISphereOfAction>());
    
    // Series
    
    internal static readonly LiveEventSeries SeriesX = new("LiveEvent Series X",
    [
        X2024,
        X2025
    ]);

    internal static readonly LiveEventSeries SeriesY = new("LiveEvent Series Y",
    [
        Y2024,
        Y2025
    ]);
    
    #endregion
    
    #region TlgAgentElementsSetup ######################################################################################

    // Needs to be 'long' instead of 'TlgUserId' for usage in InlineData() of Tests - but they implicitly convert
    
    internal const long RealTestUser_DanielGorin_TelegramId = 215737196L;
    
    internal const long Default_UserAndChatId_PrivateBotChat = 101L;
    
    internal const long UserId02 = 102L;
    internal const long UserId03 = 103L;
    
    internal const long ChatId02 = 100002L;
    internal const long ChatId03 = 100003L;
    internal const long ChatId04 = 100004L;
    internal const long ChatId05 = 100005L;
    internal const long ChatId06 = 100006L;
    internal const long ChatId07 = 100007L;
    
    // Default for tests
    internal static readonly TlgAgent PrivateBotChat_Operations =
        new(Default_UserAndChatId_PrivateBotChat,
            Default_UserAndChatId_PrivateBotChat,
            Operations);
    
    internal static readonly TlgAgent PrivateBotChat_Communications =
        new(Default_UserAndChatId_PrivateBotChat,
            Default_UserAndChatId_PrivateBotChat,
            Communications);

    internal static readonly TlgAgent PrivateBotChat_Notifications =
        new(Default_UserAndChatId_PrivateBotChat,
            Default_UserAndChatId_PrivateBotChat,
            Notifications);

    internal static readonly TlgAgent UserId02_ChatId03_Operations =
        new(UserId02,
            ChatId03,
            Operations);

    internal static readonly TlgAgent UserId02_ChatId04_Operations =
        new(UserId02,
            ChatId04,
            Operations);
    
    internal static readonly TlgAgent UserId03_ChatId06_Operations =
        new(
            UserId03,
            ChatId06,
            Operations);
    
    internal static readonly TlgAgent UserId03_ChatId06_Communications =
        new(
            UserId03,
            ChatId06,
            Communications);

    #endregion
}

