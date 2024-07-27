namespace CheckMade.Common.Model.Utils;

public interface IDomainGlossary
{
    IReadOnlyDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }
    IDictionary<CallbackId, DomainTerm> TermById { get; }
    IReadOnlyCollection<DomainTerm> GetAll(Type superType);
    
    string GetId(Type dtType);
    string GetId(Enum dtEnum);
    string GetId(DomainTerm domainTerm);
    
    UiString GetUi(Type dtType);
    UiString GetUi(Enum dtEnum);
    UiString GetUi(DomainTerm domainTerm);

    Type GetDtType(string dtId);
}