using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Data.ChatBot;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    TlgAttachmentType AttachmentType,
    Option<string> Caption);