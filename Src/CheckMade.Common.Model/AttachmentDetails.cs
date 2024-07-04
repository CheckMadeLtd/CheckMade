using CheckMade.Common.Model.ChatBot;

namespace CheckMade.Common.Model;

public record AttachmentDetails(
    Uri AttachmentUri,
    TlgAttachmentType AttachmentType,
    Option<UiString> Caption);