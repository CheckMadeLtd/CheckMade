using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record OldFormatDetailsPair(InputMessage ModelMessage, JObject OldFormatDetailsJson);