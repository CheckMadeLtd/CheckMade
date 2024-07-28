using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record ConsumablesIssue<T>(
        Guid Id,
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IReadOnlyCollection<ConsumablesItem> AffectedItems,
        Role ReportedBy,
        Option<Role> HandledBy,
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeIssue<T> where T : ITrade
{
    public IReadOnlyDictionary<IssueSummaryCategories, UiString> GetSummary()
    {
        throw new NotImplementedException();
    }

    private UiString FormatConsumableItems()
    {
        return UiConcatenate(
            Ui("Affected consumables: "),
            UiConcatenate(
                AffectedItems
                    .Select(item => UiConcatenate(
                        Glossary.GetUi(item), UiNoTranslate("; ")))
                    .ToArray()));
    }
}