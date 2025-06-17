using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Abstract.Domain.Data.Core.Submissions;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
