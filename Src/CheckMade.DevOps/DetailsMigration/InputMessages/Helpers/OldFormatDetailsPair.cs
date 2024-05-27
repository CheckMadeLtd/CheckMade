using CheckMade.Telegram.Model.DTOs;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record OldFormatDetailsPair(InputMessageDto ModelMessage, JObject OldFormatDetailsJson);