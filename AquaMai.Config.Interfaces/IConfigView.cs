namespace AquaMai.Config.Interfaces;

public interface IConfigView
{
    public void SetValue(string path, object value);
    public T GetValueOrDefault<T>(string path, T defaultValue = default);
    public bool TryGetValue<T>(string path, out T resultValue);
    public bool Remove(string path);
    public bool IsSectionEnabled(string path);
    public string ToToml();
    public IConfigView Clone();
}
