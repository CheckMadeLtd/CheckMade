using CheckMade.Common.Model.Telegram.Updates;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

internal record OldFormatDetailsPair(TelegramUpdate ModelMessage, JObject OldFormatDetailsJson);