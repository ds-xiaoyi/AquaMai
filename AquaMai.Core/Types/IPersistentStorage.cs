namespace AquaMai.Core.Types;

public interface IPersistentStorage
{
    public float GetFloat(uint playerOrId, string key, float defaultValue);
    public void SetFloat(uint playerOrId, string key, float value);
    
    public int GetInt(uint playerOrId, string key, int defaultValue);
    public void SetInt(uint playerOrId, string key, int value);
    
    public string GetString(uint playerOrId, string key, string defaultValue);
    public void SetString(uint playerOrId, string key, string value);
}