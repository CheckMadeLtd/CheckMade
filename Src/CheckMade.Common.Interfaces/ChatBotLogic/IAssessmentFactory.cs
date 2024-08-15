using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Submissions.Assessment;

namespace CheckMade.Common.Interfaces.ChatBotLogic;

public interface IAssessmentFactory
{
    Task<IAssessment> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
}