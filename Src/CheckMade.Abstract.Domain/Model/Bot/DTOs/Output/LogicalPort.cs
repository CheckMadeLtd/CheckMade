using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Core.Actors;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);