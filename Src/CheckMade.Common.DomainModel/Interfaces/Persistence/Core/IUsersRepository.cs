using CheckMade.Common.DomainModel.Data.Core.Actors;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}