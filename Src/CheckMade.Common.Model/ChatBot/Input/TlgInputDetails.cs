using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.Input;

public sealed record TlgInputDetails(
    Option<string> Text,
    Option<Uri> AttachmentTlgUri,
    Option<Uri> AttachmentInternalUri,
    Option<TlgAttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<DomainTerm> DomainTerm,
    Option<long> ControlPromptEnumCode);