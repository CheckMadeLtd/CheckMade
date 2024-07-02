using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Interfaces;

public interface ISphereOfAction
{
    string Name { get; }
    Type Trade { get; }
    SphereOfActionDetails Details { get; }
}