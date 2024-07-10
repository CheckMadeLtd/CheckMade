namespace CheckMade.Common.Model.Core.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    Type Trade { get; }
    ISphereOfActionDetails Details { get; }
}