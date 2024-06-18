using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record Role(
    string Token,
    DomainGlossary.RoleType RoleType,
    User User,
    DbRecordStatus Status = DbRecordStatus.Active);