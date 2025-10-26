using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using AquaMai.Mods.Fix;
using AquaMai.Core.Helpers;
using AquaMai.Core.Resources;
using AquaMai.Core.Types;
using AquaMai.Mods.UX.PracticeMode.Libs;
using HarmonyLib;
using Manager;
using Monitor;
using Monitor.Game;
using Process;
using UnityEngine;
using AquaMai.Config.Attributes;
using AquaMai.Config.Types;
using DB;
using MAI2.Util;
using Manager.UserDatas;

namespace AquaMai.Mods.UX.PracticeMode;

[ConfigCollapseNamespace]
[ConfigSection(
    en: "Practice Mode.",
    name: "练习模式")]
public class PracticeMode : IPlayerSettingsItem
{
    [ConfigEntry(
        name: "练习模式激活后显示提示",
        en: "Show notice when Practice Mode is activated",
        zh: "练习模式激活后显示提示")]
    public static readonly bool showNotice = true;

    [ConfigEntry(
        name: "练习模式时不保存成绩",
        en: "Do not save scores when Practice Mode is used",
        zh: "练习模式时不保存成绩")]
    public static readonly bool dontSaveScore = true;

    [ConfigEntry(
        name: "按键",
        en: "Key to show Practice Mode UI.",
        zh: "显示练习模式 UI 的按键")]
    public static readonly KeyCodeOrName key = KeyCodeOrName.Test;

    [ConfigEntry]
    public static readonly bool longPress = false;

    public static double repeatStart = -1;
    public static double repeatEnd = -1;
    public static float speed = 1;
    private static CriAtomExPlayer player;
    private static MovieMaterialMai2 movie;
    public static GameCtrl gameCtrl;
    public static bool keepNoteSpeed = false;
    
    private static bool userEnable;
    
    public static bool ignoreScore = false;
    private static UserScore oldScore;
    private static uint currentTrackNumber => GameManager.MusicTrackNumber;
    
    public static void SetRepeatEnd(double time)
    {
        if (repeatStart == -1)
        {
            MessageHelper.ShowMessage(Locale.RepeatStartTimeNotSet);
            return;
        }

        if (time < repeatStart)
        {
            MessageHelper.ShowMessage(Locale.RepeatEndTimeLessThenStartTime);
            return;
        }

        repeatEnd = time;
    }

    public static void ClearRepeat()
    {
        repeatStart = -1;
        repeatEnd = -1;
    }

    public static void SetSpeed()
    {
        if (!userEnable) return;
        
        player.SetPitch((float)(1200 * Math.Log(speed, 2)));
        player.UpdateAll();

        movie.player.SetSpeed(speed);
        gameCtrl?.ResetOptionSpeed();
    }

    private static IEnumerator SetSpeedCoroutineInner()
    {
        yield return null;
        SetSpeed();
    }

    public static void SetSpeedCoroutine()
    {
        SharedInstances.GameMainObject.StartCoroutine(SetSpeedCoroutineInner());
    }

    public static void SpeedUp()
    {
        speed += .05f;
        if (speed > 2)
        {
            speed = 2;
        }

        SetSpeed();
    }

    public static void SpeedDown()
    {
        speed -= .05f;
        if (speed < 0.05)
        {
            speed = 0.05f;
        }

        SetSpeed();
    }

    public static void SpeedReset()
    {
        speed = 1;
        SetSpeed();
    }

    public static void Seek(int addMsec)
    {
        if (!userEnable) return;
        
        // Debug feature 里面那个 timer 不能感知变速
        // 为了和魔改版本统一，polyfill 里面不修这个
        // 这里重新实现一个能感知变速的 Seek
        var msec = CurrentPlayMsec + addMsec;
        if (msec < 0)
        {
            msec = 0;
        }

        CurrentPlayMsec = msec;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCtrl), "ForceNoteCollect")]
    private static void ForceNoteCollect(NotesManager ___NoteMng)
    {
        if (!userEnable) return;
        
        foreach (NoteData note in ___NoteMng.getReader().GetNoteList())
        {
            if (note != null && note.type.isConnectSlide())
            {
                note.isJudged = true;
            }
        }
    }

    public static double CurrentPlayMsec
    {
        get => NotesManager.GetCurrentMsec() - 91;
        set
        {
            if (!userEnable) return;
            
            DebugFeature.CurrentPlayMsec = value;
            SetSpeedCoroutine();
        }
    }

    public static PracticeModeUI ui;

    [HarmonyPatch]
    public class PatchNoteSpeed
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GameManager), "GetNoteSpeed");
            yield return AccessTools.Method(typeof(GameManager), "GetTouchSpeed");
        }

        public static void Postfix(ref float __result)
        {
            if (!userEnable || !keepNoteSpeed) return;
            __result /= speed;
        }
    }

    [HarmonyPatch(typeof(GameProcess), "OnStart")]
    [HarmonyPostfix]
    public static void GameProcessPostStart(GameMonitor[] ____monitors)
    {
        repeatStart = -1;
        repeatEnd = -1;
        speed = 1;
        ui = null;
        
        ignoreScore = false;
        
        if (externalUI != null)
        {
            UnityEngine.Object.Destroy(externalUI);
            externalUI = null;
        }
        
        if (showNotice)
        {
            ____monitors[0].gameObject.AddComponent<PracticeModeNoticeUI>();
        }
    }

    [HarmonyPatch(typeof(GameProcess), "OnRelease")]
    [HarmonyPostfix]
    public static void GameProcessPostRelease()
    {
        repeatStart = -1;
        repeatEnd = -1;
        speed = 1;
        ui = null;
    }

    [HarmonyPatch(typeof(GameCtrl), "Initialize")]
    [HarmonyPostfix]
    public static void GameCtrlPostInitialize(GameCtrl __instance)
    {
        gameCtrl = __instance;
    }

# if DEBUG
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GenericProcess), "OnUpdate")]
    public static void OnGenericProcessUpdate(GenericMonitor[] ____monitors)
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ____monitors[0].gameObject.AddComponent<PracticeModeUI>();
        }
    }
# endif

    [HarmonyPatch(typeof(GameProcess), "OnUpdate")]
    [HarmonyPostfix]
    public static void GameProcessPostUpdate(GameProcess __instance, GameMonitor[] ____monitors)
    {
        if (GameManager.IsInGame && userEnable && dontSaveScore && !ignoreScore)
        {
            ignoreScore = true;
        }

        if (!userEnable) return;
        
        if (KeyListener.GetKeyDownOrLongPress(key, longPress) && ui is null)
        {
            ui = ____monitors[0].gameObject.AddComponent<PracticeModeUI>();
        }

        if (repeatStart >= 0 && repeatEnd >= 0)
        {
            if (CurrentPlayMsec >= repeatEnd)
            {
                CurrentPlayMsec = repeatStart;
            }
        }
    }

    private static float startGap = -1f;

    [HarmonyPatch(typeof(NotesManager), "StartPlay")]
    [HarmonyPostfix]
    public static void NotesManagerPostUpdateTimer(float msecStartGap)
    {
        startGap = msecStartGap;
    }

    private static bool isInAdvDemo = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AdvDemoProcess), nameof(AdvDemoProcess.OnStart))]
    public static void AdvDemoProcessOnStart()
    {
        isInAdvDemo = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AdvDemoProcess), nameof(AdvDemoProcess.OnRelease))]
    public static void AdvDemoProcessOnRelease()
    {
        isInAdvDemo = false;
    }


    [HarmonyPatch(typeof(NotesManager), "UpdateTimer")]
    [HarmonyPrefix]
    public static bool NotesManagerPostUpdateTimer(bool ____isPlaying, Stopwatch ____stopwatch, ref float ____curMSec, ref float ____curMSecPre, float ____msecStartGap)
    {
        if (!userEnable)
        {
            return true;
        }
        
        if (isInAdvDemo || GameManager.IsKaleidxScopeMode)
        {
            return true;
        }

        if (startGap != -1f)
        {
            ____curMSec = startGap;
            ____curMSecPre = startGap;
            ____stopwatch?.Reset();
            startGap = -1f;
        }
        else
        {
            ____curMSecPre = ____curMSec;
            if (____isPlaying && ____stopwatch != null && !DebugFeature.Pause)
            {
                var num = (double)____stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000.0 * speed;
                ____curMSec += (float)num;
                ____stopwatch.Reset();
                ____stopwatch.Start();
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(SoundCtrl), "Initialize")]
    [HarmonyPostfix]
    public static void SoundCtrlPostInitialize(SoundCtrl.InitParam param, Dictionary<int, object> ____players)
    {
        var wrapper = ____players[2];
        player = (CriAtomExPlayer)wrapper.GetType().GetField("Player").GetValue(wrapper);
        // var pool = new CriAtomExStandardVoicePool(1, 8, 96000, true, 2);
        // pool.AttachDspTimeStretch();
        // player.SetVoicePoolIdentifier(pool.identifier);

        // debug
        // var wrapper1 = ____players[7];
        // var player1 = (CriAtomExPlayer)wrapper1.GetType().GetField("Player").GetValue(wrapper1);
        // var pool = new CriAtomExStandardVoicePool(1, 8, 96000, true, 2);
        // pool.AttachDspTimeStretch();
        // player1.SetVoicePoolIdentifier(pool.identifier);
        // player1.SetDspTimeStretchRatio(2);
    }

    [HarmonyPatch(typeof(MovieController), "Awake")]
    [HarmonyPostfix]
    public static void MovieControllerPostAwake(MovieMaterialMai2 ____moviePlayers)
    {
        movie = ____moviePlayers;
    }

    public static void OnBeforePatch()
    {
        GameSettingsManager.RegisterSetting(new PracticeMode());
    }

    public int Sort => 60;
    public bool IsLeftButtonActive => true;
    public bool IsRightButtonActive => true;
    public string Name => "练习模式";
    public string Detail => "启用练习模式功能";
    public string SpriteFile => "UI_OPT_E_23_06";
    
    public string GetOptionValue(int player)
    {
        return userEnable ? "ON" : "OFF";
    }

    public void AddOption(int player)
    {
        userEnable = true;
        
        MessageHelper.ShowMessage(Locale.PracticeModeEnabled);
    }

    public void SubOption(int player)
    {
        userEnable = false;
        
        MessageHelper.ShowMessage(Locale.PracticeModeDisabled);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicSelectProcess), nameof(MusicSelectProcess.OnStart))]
    public static void LoadSettings()
    {
        userEnable = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserData), "UpdateScore")]
    public static bool BeforeUpdateScore(int musicid, int difficulty, uint achive, uint romVersion)
    {
        if (ignoreScore)
        {
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ResultProcess), "OnStart")]
    [HarmonyPriority(HarmonyLib.Priority.High)]
    public static bool BeforeResultProcessStart()
    {
        if (!ignoreScore)
        {
            return true;
        }
        var musicid = GameManager.SelectMusicID[0];
        var difficulty = GameManager.SelectDifficultyID[0];
        var userData = Singleton<UserDataManager>.Instance.GetUserData(0);
        oldScore = JsonUtility.FromJson<UserScore>(JsonUtility.ToJson(userData.ScoreDic[difficulty].GetValueSafe(musicid)));
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ResultProcess), "OnStart")]
    [HarmonyPriority(HarmonyLib.Priority.High)]
    public static void AfterResultProcessStart()
    {
        if (!ignoreScore)
        {
            return;
        }
        ignoreScore = false;
        var musicid = GameManager.SelectMusicID[0];
        var difficulty = GameManager.SelectDifficultyID[0];
        var score = Singleton<GamePlayManager>.Instance.GetGameScore(0, (int)currentTrackNumber - 1);
        typeof(GameScoreList).GetProperty("Achivement", BindingFlags.Public | BindingFlags.Instance)?.GetSetMethod(true)?.Invoke(score, [0m]);
        typeof(GameScoreList).GetProperty("ComboType", BindingFlags.Public | BindingFlags.Instance)?.GetSetMethod(true)?.Invoke(score, [PlayComboflagID.None]);
        typeof(GameScoreList).GetProperty("NowComboType", BindingFlags.Public | BindingFlags.Instance)?.GetSetMethod(true)?.Invoke(score, [PlayComboflagID.None]);
        score.SyncType = PlaySyncflagID.None;
        var userData = Singleton<UserDataManager>.Instance.GetUserData(0);
        var userScoreDict = userData.ScoreDic[difficulty];
        if (oldScore != null)
        {
            userScoreDict[musicid] = oldScore;
        }
        else
        {
            userScoreDict.Remove(musicid);
        }
    }

    private class PracticeModeNoticeUI : MonoBehaviour
    {
        public void OnGUI()
        {
            if (!ignoreScore) return;
            var y = Screen.height * .075f;
            var width = GuiSizes.FontSize * 20f;
            var x = GuiSizes.PlayerCenter + GuiSizes.PlayerWidth / 2f - width;
            var rect = new Rect(x, y, width, GuiSizes.LabelHeight * 2.5f);

            var labelStyle = GUI.skin.GetStyle("label");
            labelStyle.fontSize = (int)(GuiSizes.FontSize * 1.2);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Box(rect, "");
            GUI.Label(rect, userEnable ? "练习模式开启中" : "练习模式曾开启过，本曲成绩不会被上传。");
        }
    }

    private static ExternalPracticeModeUI externalUI;

    private class ExternalPracticeModeUI : MonoBehaviour
    {
        private float displayTime = 2.0f;
        private float startTime;

        public void Start()
        {
            startTime = Time.time;
        }

        public void OnGUI()
        {
            if (Time.time - startTime > displayTime)
            {
                Destroy(this);
                return;
            }

            var y = Screen.height * .075f;
            var width = GuiSizes.FontSize * 20f;
            var x = GuiSizes.PlayerCenter + GuiSizes.PlayerWidth / 2f - width;
            var rect = new Rect(x, y, width, GuiSizes.LabelHeight * 2.5f);

            var labelStyle = GUI.skin.GetStyle("label");
            labelStyle.fontSize = (int)(GuiSizes.FontSize * 1.2);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Box(rect, "");
            GUI.Label(rect, userEnable ? "练习模式已开启" : "练习模式已关闭");
        }
    }
}