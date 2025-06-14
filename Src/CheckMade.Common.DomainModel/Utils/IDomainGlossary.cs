namespace CheckMade.Common.DomainModel.Utils;

public interface IDomainGlossary
{
    IReadOnlyDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }
    IDictionary<CallbackId, DomainTerm> TermById { get; }
    IReadOnlyCollection<DomainTerm> GetAll(Type superType);
    
    string GetId(Type dtType);
    string GetId(Enum dtEnum);
    string GetId(DomainTerm domainTerm);
    string GetIdForEquallyNamedInterface(Type dtType);
    
    UiString GetUi(Type dtType);
    UiString GetUi(Enum dtEnum);
    UiString GetUi(DomainTerm domainTerm);
    
    UiString GetUi(IReadOnlyCollection<Enum> dtEnums);
    UiString GetUi(IReadOnlyCollection<DomainTerm> domainTerms);

    Type GetDtType(string dtId);
}