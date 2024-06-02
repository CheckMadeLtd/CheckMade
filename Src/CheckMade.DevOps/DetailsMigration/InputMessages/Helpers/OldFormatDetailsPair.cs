using CheckMade.Common.Model.TelegramUpdates;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record OldFormatDetailsPair(TelegramUpdateDto ModelMessage, JObject OldFormatDetailsJson);