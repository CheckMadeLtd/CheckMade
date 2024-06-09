using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Enums.UserInteraction;

namespace CheckMade.Common.Model.Tlg;

public record TlgLogicPort(Role Role, InteractionMode InteractionMode);