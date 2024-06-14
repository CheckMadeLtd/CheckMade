namespace CheckMade.Common.Model.ChatBot.Output;

public record OutputAttachmentDetails(Uri AttachmentUri, TlgAttachmentType AttachmentType, Option<UiString> Caption);