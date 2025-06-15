using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Actors;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface IUserInfo
{
    MobileNumber Mobile { get; }
    string FirstName { get; }
    Option<string> MiddleName { get; }
    string LastName { get; }
    Option<EmailAddress> Email { get; }
    LanguageCode Language { get; }
    DbRecordStatus Status { get; }

    bool Equals(IUserInfo? other);
}