using System;
using System.Collections.Generic;
using System.Reflection;
using AquaMai.Config.Attributes;
using AquaMai.Core.Helpers;
using AquaMai.Core.Resources;
using AquaMai.Core.Types;
using DB;
using HarmonyLib;
using MAI2.Util;
using Manager;
using MelonLoader;
using Monitor.Game;
using Process;
using Process.SubSequence;
using UnityEngine;
using UserOption = Manager.UserDatas.UserOption;

namespace AquaMai.Mods.Utils;

[ConfigSection(
    en: "Move answer sound",
    zh: "移动正解音。此设置提供分用户设置，请在游戏内设置中调整偏移量")]
public class MoveAnswerSound : IPlayerSettingsItem
{
    private static float[] userSettings = [0, 0];
    private static IPersistentStorage storage = new PlayerPrefsStorage();

    #region 设置界面注入

    public static void OnBeforePatch()
    {
        GameSettingsManager.RegisterSetting(new MoveAnswerSound());
    }

    public int Sort => 50;
    public bool IsLeftButtonActive => true;
    public bool IsRightButtonActive => true;
    public string Name => Locale.GameSettingsNameMoveAnswerSound;
    public string Detail => Locale.GameSettingsDetailMoveAnswerSound;
    public string SpriteFile => "UI_OPT_E_23_06";
    public string GetOptionValue(int player)
    {
        return (userSettings[player] == 0 ? GameSettingsManager.DefaultTag : GameSettingsManager.NormalTag) + userSettings[player] + Locale.MoveAnswerSoundUnit;
    }

    public void AddOption(int player)
    {
        userSettings[player]++;
    }

    public void SubOption(int player)
    {
        userSettings[player]--;
    }

    #endregion

    #region 设置存储

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicSelectProcess), nameof(MusicSelectProcess.OnStart))]
    public static void LoadSettings()
    {
        for (uint i = 0; i < 2; i++)
        {
            var userData = UserDataManager.Instance.GetUserData(i);
            if (!userData.IsEntry) continue;
            userSettings[i] = storage.GetFloat(i, "MoveAnswerSound", 0);
            MelonLogger.Msg($"玩家 {i} 的移动正解音设置为 {userSettings[i]} 毫秒");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicSelectProcess), nameof(MusicSelectProcess.OnRelease))]
    public static void SaveSettings()
    {
        for (uint i = 0; i < 2; i++)
        {
            var userData = UserDataManager.Instance.GetUserData(i);
            if (!userData.IsEntry) continue;
            storage.SetFloat(i, "MoveAnswerSound", userSettings[i]);
        }
#if DEBUG
        MelonLogger.Msg($"移动正解音设置已保存");
#endif
        PlayerPrefs.Save();
    }

    #endregion

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameCtrl), nameof(GameCtrl.UpdateCtrl))]
    public static void PreUpdateControl(GameObject ____NoteRoot, GameCtrl __instance, NotesManager ___NoteMng)
    {
        if (!____NoteRoot.activeSelf) return;
        var gameScore = Singleton<GamePlayManager>.Instance.GetGameScore(__instance.MonitorIndex);
        if (gameScore.IsTrackSkip) return;
        foreach (NoteData note in ___NoteMng.getReader().GetNoteList())
        {
            if (note == null)
            {
                continue;
            }
            if (note.type.isSlide() || note.type.isConnectSlide()) continue;
            if (NotesManager.GetCurrentMsec() - 33f - userSettings[__instance.MonitorIndex] > note.time.msec && !note.playAnsSoundHead)
            {
                Singleton<GameSingleCueCtrl>.Instance.ReserveAnswerSe(__instance.MonitorIndex);
                note.playAnsSoundHead = true;
            }
            if (note.type.isHold() && NotesManager.GetCurrentMsec() - 33f - userSettings[__instance.MonitorIndex] > note.end.msec && !note.playAnsSoundTail)
            {
                Singleton<GameSingleCueCtrl>.Instance.ReserveAnswerSe(__instance.MonitorIndex);
                note.playAnsSoundTail = true;
            }
        }
    }

    [HarmonyPatch]
    public class PatchIsAllSlide
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(NotesTypeID).GetMethod(nameof(NotesTypeID.isAllSlide), BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// 这个方法只在 GameControl.UpdateCtrl 中用了一次，所以应该可以安全的用它来屏蔽原始的正解音逻辑
        /// </summary>
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}