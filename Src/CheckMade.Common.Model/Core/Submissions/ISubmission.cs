using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Submissions;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
}