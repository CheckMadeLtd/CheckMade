using CheckMade.Common.Model;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record OldFormatDetailsPair(InputMessageDto ModelMessage, JObject OldFormatDetailsJson);