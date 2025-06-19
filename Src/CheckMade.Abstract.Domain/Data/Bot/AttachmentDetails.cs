using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Bot;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    AttachmentType AttachmentType,
    Option<string> Caption);