using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.ChatBot;

public record TlgClientPortRole(
    Role Role,
    TlgAgent TlgAgent,
    DateTime ActivationDate,
    Option<DateTime> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);