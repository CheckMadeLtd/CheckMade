using CheckMade.Common.Model.ChatBot.Input;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal record OldFormatDetailsPair(TlgInput ModelMessage, JObject OldFormatDetailsJson);