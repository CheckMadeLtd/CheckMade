namespace CheckMade.Abstract.Domain.Model.Core.CrossCutting;

public enum DbRecordStatus
{
    Active = 1,
    Historic = 90,
    SoftDeleted = 99
}