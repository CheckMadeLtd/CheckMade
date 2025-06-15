using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.GIS;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.ChatBot.Input;

public sealed record InputDetails(
    Option<string> Text,
    Option<Uri> AttachmentInternalUri,
    Option<TlgAttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<DomainTerm> DomainTerm,
    Option<long> ControlPromptEnumCode);