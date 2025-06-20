using CheckMade.Core.Model.Common.CrossCutting;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Core.Model.Common.Actors;

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