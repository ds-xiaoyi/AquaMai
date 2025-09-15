using System.Collections.Generic;
using AquaMai.Config.Attributes;
using HarmonyLib;
using Manager.UserDatas;

namespace AquaMai.Mods.GameSettings;

[ConfigSection(
    en: "Globally adjust A/B judgment or increase touch delay.",
    zh: "全局调整 A/B 判或增加触摸延迟")]
public class JudgeAdjust
{
    [ConfigEntry(
        en: "Adjust A judgment (unit same as in-game options).",
        zh: "调整 A 判（单位和游戏里一样）")]
    private static readonly double a = 0;

    [ConfigEntry(
        en: "Adjust B judgment (unit same as in-game options).",
        zh: "调整 B 判（单位和游戏里一样）")]
    private static readonly double b = 0;

    [ConfigEntry(
        en: "Increase touch delay (ms) for 1P.",
        zh: "增加触摸延迟，单位是毫秒（1P）全新队列实现，不会吃键")]
    private static readonly uint touchDelay1P = 0;

    [ConfigEntry(
        en: "Increase touch delay (ms) for 2P.",
        zh: "增加触摸延迟，单位是毫秒（2P）全新队列实现，不会吃键")]
    private static readonly uint touchDelay2P = 0;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UserOption), "GetAdjustMSec")]
    public static void GetAdjustMSec(ref float __result)
    {
        __result += (float)(a * 16.666666d);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UserOption), "GetJudgeTimingFrame")]
    public static void GetJudgeTimingFrame(ref float __result)
    {
        __result += (float)b;
    }

    private static readonly Dictionary<uint, Queue<DelayedTouchData>> _delayedTouchData = new();
    private static readonly object _lockObject = new object();

    private struct DelayedTouchData
    {
        public ulong Data;
        public uint Counter;
        public long Timestamp;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Manager.InputManager), "SetNewTouchPanel")]
    public static bool SetNewTouchPanel(uint index, ref ulong inputData, ref uint counter, ref bool __result)
    {
        var touchDelay = index switch
        {
            0 => touchDelay1P,
            1 => touchDelay2P,
            _ => 0u,
        };
        if (touchDelay <= 0)
        {
            return true;
        }

        lock (_lockObject)
        {
            var currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var dequeueCount = 0;

            if(!_delayedTouchData.ContainsKey(index))
            {
                _delayedTouchData[index] = new Queue<DelayedTouchData>();
            }

            _delayedTouchData[index].Enqueue(new DelayedTouchData
            {
                Data = inputData,
                Counter = counter,
                Timestamp = currentTime,
            });

            var ret = false;
            foreach (var data in _delayedTouchData[index])
            {
                if (currentTime - data.Timestamp < touchDelay) break;
                ret = true;
                dequeueCount++;

                inputData = data.Data;
                counter = data.Counter;
            }

            for (var i = 0; i < dequeueCount; i++)
            {
                _delayedTouchData[index].Dequeue();
            }
            
            return ret;
        }
    }
}