using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Enums;

namespace CheckMade.Common.Model.Tlg.Input;

public record TlgInputDetails(
    DateTime TlgDate,
    int TlgMessageId,
    Option<string> Text,
    Option<Uri> AttachmentTlgUri,
    Option<Uri> AttachmentInternalUri,
    Option<AttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<int> DomainCategoryEnumCode,
    Option<long> ControlPromptEnumCode);