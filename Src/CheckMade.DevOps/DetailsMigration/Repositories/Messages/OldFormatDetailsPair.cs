using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.Repositories.Messages;

internal record OldFormatDetailsPair(InputMessage ModelMessage, JObject OldFormatDetailsJson);