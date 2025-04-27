using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues;

public interface IIssue
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    Option<Role> HandledBy { get; }
    IssueStatus Status { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<IssueSummaryCategories, UiString> GetSummary();
}
