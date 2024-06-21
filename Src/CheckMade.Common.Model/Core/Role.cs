using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record Role(
    string Token,
    RoleType RoleType,
    User User,
    LiveEvent LiveEvent,
    DbRecordStatus Status = DbRecordStatus.Active);