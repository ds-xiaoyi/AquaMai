﻿using AquaMai.Config.Attributes;
using HarmonyLib;
using Monitor;
using Process;

namespace AquaMai.Mods.Tweaks.TimeSaving;

[ConfigSection(
    name: "跳过再见界面",
    en: "Skip the \"Goodbye\" screen at the end of the game.",
    zh: "跳过游戏结束的「再见」界面")]
public class SkipGoodbyeScreen
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameOverMonitor), "IsPlayEnd")]
    public static bool GameOverMonitorPlayEnd(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameOverProcess), "OnUpdate")]
    public static void GameOverProcessOnUpdate(ref GameOverProcess.GameOverSequence ____state)
    {
        if (____state == GameOverProcess.GameOverSequence.SkyChange)
        {
            ____state = GameOverProcess.GameOverSequence.Disp;
        }
    }
}
