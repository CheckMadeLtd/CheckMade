using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Data.Core.Submissions;
using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;

namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
