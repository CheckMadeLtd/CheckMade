using CheckMade.Common.Model.ChatBot.Input;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal sealed record OldFormatDetailsPair(TlgInput ModelMessage, JObject OldFormatDetailsJson);