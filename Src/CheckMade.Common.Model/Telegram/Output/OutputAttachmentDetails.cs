namespace CheckMade.Common.Model.Telegram.Output;

public record OutputAttachmentDetails(Uri AttachmentUri, TlgAttachmentType AttachmentType, Option<UiString> Caption);