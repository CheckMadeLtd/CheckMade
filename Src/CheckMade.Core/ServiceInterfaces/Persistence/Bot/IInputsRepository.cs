using System.Data.Common;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Bot;

public interface IInputsRepository
{
    Task AddAsync(Input input, Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations);
    Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(Agent agent);
    Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<Input>> GetAllLocationAsync(Agent agent, DateTimeOffset since);
    Task<IReadOnlyCollection<Input>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTimeOffset since);
    Task<IReadOnlyCollection<Input>> GetWorkflowHistoryAsync(ILiveEventInfo liveEvent, Guid workflowGuid);
    Task HardDeleteAllAsync(Agent agent);
    
    Func<DbDataReader, IDomainGlossary, Input> InputMapper { get; }
}