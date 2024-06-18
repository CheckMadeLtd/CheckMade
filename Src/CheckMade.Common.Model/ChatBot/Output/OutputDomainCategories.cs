using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.Output;

public record OutputDomainCategories
{
    private Option<IEnumerable<DomainCategories.RoleType>> RoleTypes { get; init; }
        = Option<IEnumerable<DomainCategories.RoleType>>.None();
    
    private Option<IEnumerable<DomainCategories.SanitaryOpsIssue>> SanitaryOpsIssues { get; init; }
        = Option<IEnumerable<DomainCategories.SanitaryOpsIssue>>.None();

    private Option<IEnumerable<DomainCategories.SanitaryOpsConsumable>> SanitaryOpsConsumables { get; init; }
        = Option<IEnumerable<DomainCategories.SanitaryOpsConsumable>>.None();

    private Option<IEnumerable<DomainCategories.SanitaryOpsFacility>> SanitaryOpsFacilities { get; init; }
        = Option<IEnumerable<DomainCategories.SanitaryOpsFacility>>.None();
}
    