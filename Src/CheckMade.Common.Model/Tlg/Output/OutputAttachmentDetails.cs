namespace CheckMade.Common.Model.Tlg.Output;

public record OutputAttachmentDetails(Uri AttachmentUri, TlgAttachmentType AttachmentType, Option<UiString> Caption);