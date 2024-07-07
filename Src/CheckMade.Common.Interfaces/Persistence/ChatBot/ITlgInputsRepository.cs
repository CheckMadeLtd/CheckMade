using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs);
    Task<IReadOnlyCollection<TlgInput>> GetAllHumanAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllHumanAsync(ILiveEventInfo liveEvent);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}