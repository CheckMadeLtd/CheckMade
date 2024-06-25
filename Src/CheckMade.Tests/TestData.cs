using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Core.RoleType;
using User = CheckMade.Common.Model.Core.User;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace CheckMade.Tests;

internal static class TestData
{
#region UsersSetup

    internal static readonly User IntegrationTests_User_Daniel = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "IntegrationTest",
        "_Gorin",
        new EmailAddress("daniel-integrtest-checkmade@neocortek.net"),
        LanguageCode.en);

    internal static readonly User IntegrationTests_User_Patrick = new(
        new MobileNumber("+447538521999"),
        "_Patrick",
        "IntegrationTest",
        "_Bauer",
        Option<EmailAddress>.None(), 
        LanguageCode.de);

    internal static readonly User TestUser_Daniel = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest English",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.en);
    
    internal static readonly User TestUser_Daniel_German = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest German",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.de);

#endregion
    
#region LiveEventSetup
    
    internal static readonly LiveEventVenue MockParookaVenue = new("Mock Venue near Cologne");

    internal static readonly LiveEvent MockParooka2024 = new("Mock Parookaville 2024",
        new DateTime(2024, 07, 19, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 07, 22, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        MockParookaVenue);

    internal static readonly LiveEvent MockParooka2025 = new("Mock Parookaville 2025",
        new DateTime(2025, 07, 18, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 07, 21, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        MockParookaVenue);

    internal static readonly LiveEventSeries MockParookaSeries = 
        new("Mock Parookaville Series", 
            new List<LiveEvent> 
            {
                MockParooka2024,
                MockParooka2025
            });

    internal static readonly LiveEventVenue MockHurricaneVenue = new("Mock Venue near Bremen");
    
    internal static readonly LiveEvent MockHurricane2024 = new("Mock Hurricane 2024",
        new DateTime(2024, 06, 21, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 06, 24, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        MockHurricaneVenue);

    internal static readonly LiveEvent MockHurricane2025 = new("Mock Hurricane 2025",
        new DateTime(2025, 06, 20, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 06, 23, 18, 00, 00, DateTimeKind.Utc),
        new List<IRoleInfo>(),
        MockHurricaneVenue);

    internal static readonly LiveEventSeries MockHurricaneSeries =
        new("Mock Hurricane Series",
            new List<LiveEvent>
            {
                MockHurricane2024,
                MockHurricane2025
            });
    
#endregion
    
#region RoleSetup
    
    internal static readonly Role IntegrationTests_Role_Default = new(
        "RAAAA1",
        SanitaryOps_Inspector,
        IntegrationTests_User_Daniel,
        new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsAdmin_AtMockParooka2024_Default = 
        new("RVB70T",
            SanitaryOps_Admin, 
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsInspector1 = 
        new("R3UDXW",
            SanitaryOps_Inspector,
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsEngineer1 = 
        new("R3UED8", 
            SanitaryOps_Engineer,
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsCleanLead1_German = 
        new("R2JXNM",
            SanitaryOps_CleanLead,
            TestUser_Daniel_German, // German! Relied on by unit tests!
            new LiveEventInfo(MockParooka2024));

    internal static readonly Role SanitaryOpsObserver1 = 
        new("RYEATF",
            SanitaryOps_Observer,
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsInspector_AtMockHurricane2024_German = 
        new("RMAM8S",
            SanitaryOps_Inspector,
            TestUser_Daniel_German,
            new LiveEventInfo(MockHurricane2024));
    
    internal static readonly Role SanitaryOpsEngineer2 = 
        new("RP4XPK",
            SanitaryOps_Engineer,
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsCleanLead2 = 
        new("RI8MJ1",
            SanitaryOps_CleanLead,
            TestUser_Daniel, // English! Relied on by unit tests!
            new LiveEventInfo(MockParooka2024));
    
    internal static readonly Role SanitaryOpsObserver2 = 
        new("R67CMC",
            SanitaryOps_Observer,
            TestUser_Daniel,
            new LiveEventInfo(MockParooka2024));
    
#endregion

#region TlgAgentElementsSetup

    // Needs to be 'long' instead of 'TlgUserId' for usage in InlineData() of Tests - but they implicitly convert
    internal const long TestUser_DanielGorin_TelegramId = 215737196L;
    
    internal const long TestUserId01_PrivateChat_Default = 101L;
    internal const long TestUserId02 = 102L;
    internal const long TestUserId03 = 103L;
    
    internal const long TestChatId01_PrivateChat_Default = 101L;
    internal const long TestChatId02 = 100002L;
    internal const long TestChatId03 = 100003L;
    internal const long TestChatId04 = 100004L;
    internal const long TestChatId05 = 100005L;
    internal const long TestChatId06 = 100006L;
    internal const long TestChatId07 = 100007L;
    internal const long TestChatId08 = 100008L;
    internal const long TestChatId09 = 100009L;

    internal static readonly TlgAgent TlgAgentWithHistoricRoleBindingOnly =
        new(TestUserId02, TestChatId03, Operations);

    // ToDo: create more TlgAgents with names and use them for rolebindings?!
    
#endregion

#region TlgAgentRoleBindingsSetup

    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsAdmin_Default = 
        new(
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId01_PrivateChat_Default, Operations), 
            DateTime.UtcNow, Option<DateTime>.None());

    // ToDo: after everything compiles and tests pass, change the TEstChatId_02 to be the same as the UserId.
    // better yet, introduce a ChatId which by name indicates that it's the same as some other UserId!
    // I.e. where the sameness becomes a feature transported in the name's semantics.
    // And of course make it a named TlgAgent just like we did for the HistoricOnly one!
    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_OperationsMode =
        new(
            SanitaryOpsInspector1,
            new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId02, Operations),
            DateTime.UtcNow, Option<DateTime>.None());
    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_CommunicationsMode =
        new(
            SanitaryOpsInspector1,
            new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId02, Communications),
            DateTime.UtcNow, Option<DateTime>.None());
    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsInspector1_InPrivateChat_NotificationsMode =
        new(
            SanitaryOpsInspector1,
            new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId02, Notifications),
            DateTime.UtcNow, Option<DateTime>.None());

    
    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsEngineer1_HistoricOnly =
        new(
            SanitaryOpsEngineer1,
            TlgAgentWithHistoricRoleBindingOnly,
            new DateTime(1999, 01, 01),
            new DateTime(1999, 02, 02),
            DbRecordStatus.Historic);

    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsCleanLead1_German =
        new(
            SanitaryOpsCleanLead1_German,
            new TlgAgent(TestUserId02, TestChatId04, Operations),
            DateTime.UtcNow, Option<DateTime>.None());

    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsObserver1 =
        new(
            SanitaryOpsObserver1,
            new TlgAgent(TestUserId03, TestChatId05, Operations),
            DateTime.UtcNow, Option<DateTime>.None());

    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsEngineer2_OnlyCommunicationsMode =
        new(
            SanitaryOpsEngineer2,
            new TlgAgent(TestUserId03, TestChatId06, Communications),
            DateTime.UtcNow, Option<DateTime>.None());

    internal static readonly TlgAgentRoleBind RoleBindFor_SanitaryOpsCleanLead2 =
        new(
            SanitaryOpsCleanLead2,
            new TlgAgent(TestUserId03, TestChatId07, Operations),
            DateTime.UtcNow, Option<DateTime>.None());
    
    // No TlgAgentRoleBind for role 'Inspector2' available on purpose for Unit Tests!

    #endregion
}




























