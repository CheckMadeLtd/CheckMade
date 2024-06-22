using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
using static CheckMade.Common.Model.Core.RoleType;
using User = CheckMade.Common.Model.Core.User;
// ReSharper disable MemberCanBePrivate.Global

namespace CheckMade.Tests;

// Two different object graphs for testing:
// 1 From the direction of TlgAgentRepository i.e. with Roles containing a LiveEvent: Object-Graph 1
// 2 From the direction of LiveEventSeries i.e. with each LiveEvent containing Roles: Object-Graph 2
// But leave as many entities as possible here able to stand on their own for isolated unit tests that don't need
// either of the full object graphs ?!

public static class TestData
{
    internal static readonly User IntegrationTestsUserDanielEnglish = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "IntegrationTest",
        "_Gorin",
        new EmailAddress("daniel-integrtest-checkmade@neocortek.net"),
        LanguageCode.en);

    internal static readonly User IntegrationTestsUserPatrick = new(
        new MobileNumber("+447538521999"),
        "_Patrick",
        "IntegrationTest",
        "_Bauer",
        Option<EmailAddress>.None(), 
        LanguageCode.de);

    internal static readonly User TestUserDanielEn = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest English",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.en);
    
    internal static readonly User TestUserDanielDe = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest German",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.de);

    internal static readonly LiveEventVenue MockParookaVenue = new("Mock Venue near Cologne");

    internal static readonly LiveEvent MockParooka2024NoRoles = new("Mock Parookaville 2024",
        new DateTime(2024, 07, 19, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 07, 22, 18, 00, 00, DateTimeKind.Utc),
        new List<Role>(),
        MockParookaVenue);

    internal static readonly LiveEvent MockParooka2025NoRoles = new("Mock Parookaville 2025",
        new DateTime(2025, 07, 18, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 07, 21, 18, 00, 00, DateTimeKind.Utc),
        new List<Role>(),
        MockParookaVenue);

    internal static readonly LiveEventSeries MockParookaSeries = 
        new("Mock Parookaville Series", 
            new List<LiveEvent> 
            {
                MockParooka2024NoRoles,
                MockParooka2025NoRoles
            });
    
    internal static readonly LiveEventVenue MockHurricaneVenue = new("Mock Venue near Bremen");
    
    internal static readonly LiveEvent MockHurricane2024NoRoles = new("Mock Hurricane 2024",
        new DateTime(2024, 06, 21, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2024, 06, 24, 18, 00, 00, DateTimeKind.Utc),
        new List<Role>(),
        MockHurricaneVenue);

    internal static readonly LiveEvent MockHurricane2025NoRoles = new("Mock Hurricane 2025",
        new DateTime(2025, 06, 20, 10, 00, 00, DateTimeKind.Utc),
        new DateTime(2025, 06, 23, 18, 00, 00, DateTimeKind.Utc),
        new List<Role>(),
        MockHurricaneVenue);

    internal static readonly LiveEventSeries MockHurricaneSeries =
        new("Mock Hurricane Series",
            new List<LiveEvent>
            {
                MockHurricane2024NoRoles,
                MockHurricane2025NoRoles
            });
    
    
    // The below Roles all point to a real (rather than Proxy) LiveEvent, thereby representing the Object-graph 1. 
    
    internal static readonly Role IntegrationTestsRoleEnglish = new(
        "RAAAA1",
        SanitaryOps_Inspector,
        IntegrationTestsUserDanielEnglish,
        MockParooka2024NoRoles);
    
    internal static readonly Role DanielIsSanitaryOpsAdminAtMockParooka2024 = 
        new("RVB70T",
            SanitaryOps_Admin, 
            TestUserDanielEn,
            MockParooka2024NoRoles);
    
    internal static readonly Role SanitaryOpsInspector1 = new("R3UDXW", SanitaryOps_Inspector, TestUserDanielEn, MockParooka2024NoRoles);
    internal static readonly Role SanitaryOpsEngineer1 = new("R3UED8", SanitaryOps_Engineer, TestUserDanielEn, MockParooka2024NoRoles);
    
    internal static readonly Role SanitaryOpsCleanLead1German = 
        new("R2JXNM",
            SanitaryOps_CleanLead,
            TestUserDanielDe, // German! Relied on by unit tests!
            MockParooka2024NoRoles);

    internal static readonly Role SanitaryOpsObserver1 = new("RYEATF", SanitaryOps_Observer, TestUserDanielEn, MockParooka2024NoRoles);
    
    internal static readonly Role DanielIsSanitaryOpsInspectorAtMockHurricane2024 = 
        new("RMAM8S",
            SanitaryOps_Inspector,
            TestUserDanielDe,
            MockParooka2024NoRoles);
    
    internal static readonly Role SanitaryOpsEngineer2 = new("RP4XPK", SanitaryOps_Engineer, TestUserDanielEn, MockParooka2024NoRoles);
    
    internal static readonly Role SanitaryOpsCleanLead2English = 
        new("RI8MJ1",
            SanitaryOps_CleanLead,
            TestUserDanielEn, // English! Relied on by unit tests!
            MockParooka2024NoRoles);
    
    internal static readonly Role SanitaryOpsObserver2 = new("R67CMC", SanitaryOps_Observer, TestUserDanielEn, MockParooka2024NoRoles);
    
    // Needs to be 'long' instead of 'TlgUserId' for usage in InlineData() of Tests - but they implicitly convert
    internal const long TestUserDanielGorinTelegramId = 215737196L;
    internal const long TestUserId_01 = 101L;
    internal const long TestUserId_02 = 102L;
    internal const long TestUserId_03 = 103L;
    
    internal const long TestChatId_01 = 100001L;
    internal const long TestChatId_02 = 100002L;
    internal const long TestChatId_03 = 100003L;
    internal const long TestChatId_04 = 100004L;
    internal const long TestChatId_05 = 100005L;
    internal const long TestChatId_06 = 100006L;
    internal const long TestChatId_07 = 100007L;
    internal const long TestChatId_08 = 100008L;
    internal const long TestChatId_09 = 100009L;
}