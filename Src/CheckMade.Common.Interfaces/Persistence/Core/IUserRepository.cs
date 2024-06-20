using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IUserRepository
{
    Task UpdateLanguageSettingAsync(User user, LanguageCode newLanguage);
}