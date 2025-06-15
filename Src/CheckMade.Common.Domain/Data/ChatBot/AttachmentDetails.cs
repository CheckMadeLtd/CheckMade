using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.ChatBot;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    TlgAttachmentType AttachmentType,
    Option<string> Caption);