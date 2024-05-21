using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.Repositories.Messages;

internal record MessageOldFormatDetailsPair(InputMessage ModelMessage, JObject OldFormatDetailsJson);