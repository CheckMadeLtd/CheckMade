using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Structs;
using static CheckMade.Common.Model.Core.RoleType;
using User = CheckMade.Common.Model.Core.User;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace CheckMade.Tests.Utils;

internal static class TestData
{
    #region UsersSetup #################################################################################################

    // Needs to be in-sync with seeding script
    internal static readonly User IntegrationTests_DanielEn = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "IntegrationTest",
        "_Gorin",
        new EmailAddress("daniel-integrtest-checkmade@neocortek.net"),
        LanguageCode.en,
        new List<IRoleInfo>());

    // Needs to be in-sync with seeding script
    internal static readonly User IntegrationTests_PatrickDe = new(
        new MobileNumber("+447538521999"),
        "_Patrick",
        "IntegrationTest",
        "_Bauer",
        Option<EmailAddress>.None(), 
        LanguageCode.de,
        new List<IRoleInfo>());

    internal static readonly User DanielEn = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest English",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.en,
        new List<IRoleInfo>());
    
    internal static readonly User DanielDe = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest German",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.de,
        new List<IRoleInfo>());

    #endregion
    
    #region LiveEventSetup #############################################################################################
    
    internal static readonly LiveEventVenue Venue1 = new("Venue1 near Cologne");

    internal static readonly LiveEvent X2024 = new("LiveEvent X 2024",
        new DateTime(2024, 07, 19, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 07, 22, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        Venue1);

    internal static readonly LiveEvent X2025 = new("LiveEvent X 2025",
        new DateTime(2025, 07, 18, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 07, 21, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        Venue1);

    internal static readonly LiveEventSeries SeriesX = new("X LiveEvent Series", 
        new List<LiveEvent> 
        {
            X2024,
            X2025
        });

    internal static readonly LiveEventVenue Venue2 = new("Venue2 near Bremen");
    
    internal static readonly LiveEvent Y2024 = new("LiveEvent Y 2024",
        new DateTime(2024, 06, 21, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 06, 24, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        Venue2);

    internal static readonly LiveEvent Y2025 = new("LiveEvent Y 2025",
        new DateTime(2025, 06, 20, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 06, 23, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        Venue2);

    internal static readonly LiveEventSeries SeriesY = new("Y LiveEvent Series",
        new List<LiveEvent>
        {
            Y2024,
            Y2025
        });
    
    #endregion
    
    #region RoleSetup ##################################################################################################
    
    // Needs to be in-sync with seeding script    
    internal static readonly Role IntegrationTests_SOpsInspector_DanielEn_X2024 = new(
        "RAAAA1",
        SanitaryOps_Inspector,
        new UserInfo(IntegrationTests_DanielEn),
        new LiveEventInfo(X2024));
    
    // Default
    internal static readonly Role SOpsAdmin_DanielEn_X2024 = 
        new("RVB70T",
            SanitaryOps_Admin, 
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024));
    
    // HasRoleBindings_ForAllModes
    internal static readonly Role SOpsInspector_DanielEn_X2024 = 
        new("R3UDXW",
            SanitaryOps_Inspector,
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024));
    
    // German! Relied on by unit tests!
    internal static readonly Role SOpsCleanLead_DanielDe_X2024 = 
        new("R2JXNM",
            SanitaryOps_CleanLead,
            new UserInfo(DanielDe),
            new LiveEventInfo(X2024));

    internal static readonly Role SOpsObserver_DanielEn_X2024 = 
        new("RYEATF",
            SanitaryOps_Observer,
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024));
    
    // HasNoBindings_German
    internal static readonly Role SOpsInspector_DanielDe_X2024 = 
        new("RMAM8S",
            SanitaryOps_Inspector,
            new UserInfo(DanielDe),
            new LiveEventInfo(X2024));
    
    // HasBindOnlyIn_CommunicationsMode
    internal static readonly Role SOpsEngineer_DanielEn_X2024 = 
        new("RP4XPK",
            SanitaryOps_Engineer,
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024));
    
    // English! Relied on by unit tests!
    internal static readonly Role SOpsCleanLead_DanielEn_X2024 = 
        new("RI8MJ1",
            SanitaryOps_CleanLead,
            new UserInfo(DanielEn), 
            new LiveEventInfo(X2024));
    
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
    
    // Default
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

    // TlgAgent_HasOnly_HistoricRoleBind
    internal static readonly TlgAgent UserId02_ChatId03_Operations =
        new(UserId02,
            ChatId03,
            Operations);

    // TlgAgent_Of_SanitaryOpsCleanLead1_ChatGroup_German
    internal static readonly TlgAgent UserId02_ChatId04_Operations =
        new(UserId02,
            ChatId04,
            Operations);
    
    // TlgAgent_of_SanitaryOpsEngineer2_OperationsMode
    internal static readonly TlgAgent UserId03_ChatId06_Operations =
        new(
            UserId03,
            ChatId06,
            Operations);
    
    // TlgAgent_of_SanitaryOpsEngineer2_CommunicationsMode
    internal static readonly TlgAgent UserId03_ChatId06_Communications =
        new(
            UserId03,
            ChatId06,
            Communications);
    
    #endregion

    // #region TlgAgentRoleBindingsSetup ##################################################################################

    // internal static readonly TlgAgentRoleBind RoleBindFor_IntegrationTests_Role_Default =
    //     new(
    //         IntegrationTests_SOpsInspector_DanielEn_X2024,
    //         PrivateBotChat_Operations,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsAdmin_Default = 
    //     new(
    //         SOpsAdmin_DanielEn_X2024, 
    //         PrivateBotChat_Operations, 
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_OperationsMode =
    //     new(
    //         SOpsInspector_DanielEn_X2024,
    //         PrivateBotChat_Operations,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_CommunicationsMode =
    //     new(
    //         SOpsInspector_DanielEn_X2024,
    //         PrivateBotChat_Communications,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_NotificationsMode =
    //     new(
    //         SOpsInspector_DanielEn_X2024,
    //         PrivateBotChat_Notifications,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsEngineer1_HistoricOnly =
    //     new(
    //         SOpsEngineer_DanielEn_X2024,
    //         UserId02_ChatId03_Operations,
    //         new DateTime(1999, 01, 01),
    //         new DateTime(1999, 02, 02),
    //         DbRecordStatus.Historic);
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsCleanLead1_German =
    //     new(
    //         SOpsCleanLead_DanielDe_X2024,
    //         UserId02_ChatId04_Operations,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsEngineer2_OnlyCommunicationsMode =
    //     new(
    //         SOpsEngineer_DanielEn_X2024,
    //         UserId03_ChatId06_Communications,
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsObserver1 =
    //     new(
    //         SOpsObserver_DanielEn_X2024,
    //         new TlgAgent(UserId03, ChatId05, Operations),
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsCleanLead2 =
    //     new(
    //         SOpsCleanLead_DanielEn_X2024,
    //         new TlgAgent(UserId03, ChatId07, Operations),
    //         DateTime.UtcNow, Option<DateTime>.None());
    //
    // #endregion
}

