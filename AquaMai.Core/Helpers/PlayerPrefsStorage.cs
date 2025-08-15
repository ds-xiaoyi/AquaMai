using AquaMai.Core.Types;
using Manager;
using UnityEngine;

namespace AquaMai.Core.Helpers;

public class PlayerPrefsStorage : IPersistentStorage
{
    private uint getPlayerId(uint playerOrId)
    {
        if (playerOrId is 0 or 1)
        {
            var userData = UserDataManager.Instance.GetUserData(playerOrId);
            return userData.AimeId.Value;
        }
        return playerOrId;
    }
    public float GetFloat(uint playerOrId, string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat($"AquaMai{key}:{getPlayerId(playerOrId)}", defaultValue);
    }
    public void SetFloat(uint playerOrId, string key, float value)
    {
        PlayerPrefs.SetFloat($"AquaMai{key}:{getPlayerId(playerOrId)}", value);
        PlayerPrefs.Save();
    }
    public int GetInt(uint playerOrId, string key, int defaultValue)
    {
        return PlayerPrefs.GetInt($"AquaMai{key}:{getPlayerId(playerOrId)}", defaultValue);
    }
    public void SetInt(uint playerOrId, string key, int value)
    {
        PlayerPrefs.SetInt($"AquaMai{key}:{getPlayerId(playerOrId)}", value);
        PlayerPrefs.Save();
    }
    public string GetString(uint playerOrId, string key, string defaultValue)
    {
        return PlayerPrefs.GetString($"AquaMai{key}:{getPlayerId(playerOrId)}", defaultValue);
    }
    public void SetString(uint playerOrId, string key, string value)
    {
        PlayerPrefs.SetString($"AquaMai{key}:{getPlayerId(playerOrId)}", value);
        PlayerPrefs.Save();
    }
}