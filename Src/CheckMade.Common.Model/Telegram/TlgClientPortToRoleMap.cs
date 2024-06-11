using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Telegram;

public record TlgClientPortToRoleMap(
    Role Role,
    TlgClientPort ClientPort,
    DateTime ActivationDate,
    Option<DateTime> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);