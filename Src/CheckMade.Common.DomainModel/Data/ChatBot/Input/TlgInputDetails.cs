using CheckMade.Common.DomainModel.Data.Core.GIS;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Data.ChatBot.Input;

public sealed record TlgInputDetails(
    Option<string> Text,
    Option<Uri> AttachmentInternalUri,
    Option<TlgAttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<DomainTerm> DomainTerm,
    Option<long> ControlPromptEnumCode);