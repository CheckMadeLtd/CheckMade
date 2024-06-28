using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record Role(
        string Token,
        RoleType RoleType,
        IUserInfo ByUser,
        ILiveEventInfo AtLiveEvent,
        DbRecordStatus Status = DbRecordStatus.Active)
    : IRoleInfo
{
    public Role(IRoleInfo roleInfo, IUserInfo userInfo, ILiveEventInfo liveEventInfo)
        : this(
            roleInfo.Token,
            roleInfo.RoleType,
            userInfo,
            liveEventInfo,
            roleInfo.Status)
    {
    }
}