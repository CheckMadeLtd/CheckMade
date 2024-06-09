using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Tlg;

public record TlgClientPortToRoleMap(
    Role Role,
    TlgClientPort ClientPort,
    DateTime ActivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);