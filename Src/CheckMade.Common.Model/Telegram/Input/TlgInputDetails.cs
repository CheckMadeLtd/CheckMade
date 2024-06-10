using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.Telegram.Input;

public record TlgInputDetails(
    DateTime TlgDate,
    int TlgMessageId,
    Option<string> Text,
    Option<Uri> AttachmentTlgUri,
    Option<Uri> AttachmentInternalUri,
    Option<TlgAttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<int> DomainCategoryEnumCode,
    Option<long> ControlPromptEnumCode);