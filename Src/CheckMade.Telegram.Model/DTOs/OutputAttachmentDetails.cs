using CheckMade.Common.Model.Core.Enums;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputAttachmentDetails(Uri AttachmentUri, AttachmentType AttachmentType, Option<UiString> Caption);