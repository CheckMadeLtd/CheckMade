using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DataMigration.Repositories;

internal record MessageOldFormatDetailsPair(InputMessage ModelMessage, JObject OldFormatDetailsJson);