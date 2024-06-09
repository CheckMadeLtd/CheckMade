using CheckMade.Common.Model.Enums;

namespace CheckMade.Common.Model.Tlg.Output;

public record OutputAttachmentDetails(Uri AttachmentUri, AttachmentType AttachmentType, Option<UiString> Caption);