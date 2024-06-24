using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record RoleInfo(
        string Token,
        RoleType RoleType,
        User User,
        DbRecordStatus Status) 
    : IRoleInfo;