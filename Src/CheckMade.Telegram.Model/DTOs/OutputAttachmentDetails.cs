using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputAttachmentDetails(Uri AttachmentUri, AttachmentType AttachmentType, Option<UiString> Caption);