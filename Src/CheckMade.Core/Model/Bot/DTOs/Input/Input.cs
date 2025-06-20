﻿using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Bot.DTOs.Input;

public sealed record Input(
    DateTimeOffset TimeStamp,
    MessageId MessageId, 
    Agent Agent,
    InputType InputType,
    Option<IRoleInfo> OriginatorRole,
    Option<ILiveEventInfo> LiveEventContext,
    Option<ResultantWorkflowState> ResultantState,
    Option<Guid> EntityGuid,
    Option<string> CallbackQueryId,
    InputDetails Details);