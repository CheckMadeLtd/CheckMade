using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record User(
        MobileNumber Mobile,
        string FirstName,
        Option<string> MiddleName,
        string LastName,
        Option<EmailAddress> Email,
        LanguageCode Language,
        DbRecordStatus Status = DbRecordStatus.Active);