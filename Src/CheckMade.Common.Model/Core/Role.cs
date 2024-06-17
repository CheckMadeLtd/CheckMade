using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record Role(
    string Token,
    RoleType RoleType,
    User User,
    DbRecordStatus Status = DbRecordStatus.Active);