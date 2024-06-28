using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;

namespace CheckMade.Tests.Utils;

internal static class MockRepositoryUtils
{
    internal static TlgAgentRoleBind GetNewRoleBind(
        Role role, 
        TlgAgent tlgAgent)
    {
        return new TlgAgentRoleBind(
            role,
            tlgAgent,
            DateTime.UtcNow,
            Option<DateTime>.None());
    }
}