namespace CheckMade.Common.Model.Telegram;

public enum DbRecordStatus
{
    Active = 0, // Default
    Historic = 90,
    SoftDeleted = 99
}