using CheckMade.Common.Model.Tlg.Input;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.TlgDetailsMigration.Helpers;

internal record OldFormatDetailsPair(TlgInput ModelMessage, JObject OldFormatDetailsJson);