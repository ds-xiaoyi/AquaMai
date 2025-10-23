﻿using AquaMai.Config.Attributes;
using AquaMai.Config.Types;
using AquaMai.Core.Helpers;
using HarmonyLib;
using MAI2.Util;
using Manager;
using MelonLoader;
using Process;

namespace AquaMai.Mods.UX;

[ConfigSection(
    name: "一键重开和跳过",
    en: "One key to retry (1.30+) or skip current chart in gameplay.",
    zh: "在游戏中途一键重试（1.30+）或跳过当前谱面")]
public class OneKeyRetrySkip
{
    [ConfigEntry("重开按键")]
    public static readonly KeyCodeOrName retryKey = KeyCodeOrName.Service;

    [ConfigEntry("重开长按")]
    public static readonly bool retryLongPress = false;

    [ConfigEntry("跳关按键")]
    public static readonly KeyCodeOrName skipKey = KeyCodeOrName.Service;

    [ConfigEntry("跳关长按")]
    public static readonly bool skipLongPress = true;

    private static bool dirty = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameProcess), "OnStart")]
    public static void PostGameProcessStart()
    {
#if DEBUG
        MelonLogger.Msg("[OneKeyRetrySkip] Dirty flag reset");
#endif
        dirty = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameProcess), "OnUpdate")]
    public static void PostGameProcessUpdate(GameProcess __instance, Message[] ____message, ProcessDataContainer ___container)
    {
        if (dirty) return;

        if (KeyListener.GetKeyDownOrLongPress(skipKey, skipLongPress))
        {
#if DEBUG
            MelonLogger.Msg("[OneKeyRetrySkip] Skip key pressed.");
#endif
            dirty = true;
            var traverse = Traverse.Create(__instance);
            ___container.processManager.SendMessage(____message[0]);
            Singleton<GamePlayManager>.Instance.SetSyncResult(0);
            traverse.Method("SetRelease").GetValue();
        }

        else if (KeyListener.GetKeyDownOrLongPress(retryKey, retryLongPress) && GameInfo.GameVersion >= 23000)
        {
#if DEBUG
            MelonLogger.Msg("[OneKeyRetrySkip] Retry key pressed.");
#endif
            dirty = true;
            // This is original typo in Assembly-CSharp
            Singleton<GamePlayManager>.Instance.SetQuickRetryFrag(flag: true);
        }
    }
}