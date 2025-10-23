namespace AquaMai.Config.Interfaces;

public interface IConfigEntryAttribute
{
    IConfigComment Comment { get; }
    bool HideWhenDefault { get; }
}