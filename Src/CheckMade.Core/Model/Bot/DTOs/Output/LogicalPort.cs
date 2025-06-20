using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Common.Actors;

namespace CheckMade.Core.Model.Bot.DTOs.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);