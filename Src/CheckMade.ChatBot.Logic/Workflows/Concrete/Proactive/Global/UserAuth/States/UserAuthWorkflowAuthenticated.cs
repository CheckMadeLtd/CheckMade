using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;

internal interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

internal sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;