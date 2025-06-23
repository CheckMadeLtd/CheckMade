using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Bot.DTOs.Inputs;

public sealed record InputDetails(
    Option<string> Text,
    Option<Uri> AttachmentInternalUri,
    Option<AttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<DomainTerm> DomainTerm,
    Option<long> ControlPromptEnumCode);