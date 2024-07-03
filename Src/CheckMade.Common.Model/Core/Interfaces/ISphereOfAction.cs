namespace CheckMade.Common.Model.Core.Interfaces;

public interface ISphereOfAction
{
    string Name { get; }
    Type Trade { get; }
    ISphereOfActionDetails Details { get; }
}