using CheckMade.Common.Model.Core.Issues.Concrete;

namespace CheckMade.Common.Model.Core.Issues;

internal interface IIssueWithEvidence
{
    IssueEvidence Evidence { get; }
}