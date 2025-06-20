using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;

public sealed record InputDetails(
    Option<string> Text,
    Option<Uri> AttachmentInternalUri,
    Option<AttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<DomainTerm> DomainTerm,
    Option<long> ControlPromptEnumCode);