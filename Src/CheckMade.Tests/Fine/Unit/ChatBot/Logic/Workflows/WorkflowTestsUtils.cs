using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

internal static class WorkflowTestsUtils
{
    internal static (
        ITestUtils utils,
        IWorkflowUtils workflowUtils,
        Mock<ITlgClientPortRoleRepository> mockPortRolesRepo, 
        IRoleRepository mockRoleRepo, 
        DateTime baseDateTime) 
        GetBasicTestingServices(IServiceProvider sp) =>
        (sp.GetRequiredService<ITestUtils>(),
            sp.GetRequiredService<IWorkflowUtils>(),
            sp.GetRequiredService<Mock<ITlgClientPortRoleRepository>>(),
            sp.GetRequiredService<IRoleRepository>(),
            DateTime.UtcNow);
}