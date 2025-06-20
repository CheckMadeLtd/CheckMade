using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Core.Actors;

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