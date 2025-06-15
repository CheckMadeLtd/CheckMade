using CheckMade.Common.DomainModel.Core;
using CheckMade.Common.DomainModel.Core.Structs;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.DomainModel.Interfaces.Core;

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