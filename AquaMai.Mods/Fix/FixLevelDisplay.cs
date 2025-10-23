﻿using AquaMai.Core.Attributes;
using AquaMai.Config.Attributes;
using AquaMai.Core.Helpers;
using HarmonyLib;
using MAI2.Util;
using Manager;
using Monitor;
using Monitor.MusicSelect.ChainList;
using UnityEngine;

namespace AquaMai.Mods.Fix;

[EnableGameVersion(24000)]
[ConfigSection(exampleHidden: true, defaultOn: true)]
public class FixLevelDisplay
{
    // Fix wrong position of level number's display for music levels with non-consistant display level and rate level (difficuly constant)
    // Stock game charts have no such inconsistency, but custom charts may have (e.g. 10+ but unrated)

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicChainCardObejct), "SetLevel")]
    private static void FixLevelShiftMusicChainCardObejct(
        MusicLevelID levelID,
        SpriteCounter ____digitLevel,
        SpriteCounter ____doubleDigitLevel,
        bool utage,
        GameObject ____difficultyUtageQuesionMarkSingleDigit,
        GameObject ____difficultyUtageQuesionMarkDoubleDigit)
    {
        // 在 KLD 表门和里门不应用修改
        if (GameInfo.GameVersion >= 25500 && GameManager.IsKaleidxScopeMode)
        {
            if (Singleton<KaleidxScopeManager>.Instance.gateId == 8 ||
                Singleton<KaleidxScopeManager>.Instance.gateId == 10)
            {
                return;
            }
        }

        switch (levelID)
        {
            case > MusicLevelID.Level9P:
                ____digitLevel.gameObject.SetActive(value: false);
                ____doubleDigitLevel.gameObject.SetActive(value: true);
                ____doubleDigitLevel.ChangeText(levelID.GetLevelNum().PadRight(3));
                break;
            case >= MusicLevelID.None:
                ____digitLevel.gameObject.SetActive(value: true);
                ____doubleDigitLevel.gameObject.SetActive(value: false);
                ____digitLevel.ChangeText(levelID.GetLevelNum().PadRight(2));
                break;
        }

        if (!utage) return;
        switch (levelID)
        {
            case > MusicLevelID.Level9P:
                ____difficultyUtageQuesionMarkSingleDigit.SetActive(value: false);
                ____difficultyUtageQuesionMarkDoubleDigit.SetActive(value: true);
                break;
            case >= MusicLevelID.None:
                ____difficultyUtageQuesionMarkSingleDigit.SetActive(value: true);
                ____difficultyUtageQuesionMarkDoubleDigit.SetActive(value: false);
                break;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SingleResultCardController), "SetLevel")]
    private static void FixLevelShiftSingleResultCardController(
        MusicLevelID levelID,
        bool isUtage,
        ref SpriteCounter ____difficultySingle,
        ref SpriteCounter ____difficultyDouble,
        GameObject ____utageQuestionMarkSingleDigit,
        GameObject ____utageQuestionMarkDoubleDigit)
    {
        FixLevelShiftMusicChainCardObejct(levelID, ____difficultySingle, ____difficultyDouble, isUtage, ____utageQuestionMarkSingleDigit, ____utageQuestionMarkDoubleDigit);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TotalResultPlayer), "SetLevel")]
    private static void FixLevelShiftTotalResultPlayer(
        MusicLevelID levelID,
        bool isUtage,
        ref SpriteCounter ____difficultySingle,
        ref SpriteCounter ____difficultyDouble,
        GameObject ____utageQuestionMarkSingleDigit,
        GameObject ____utageQuestionMarkDoubleDigit)
    {
        FixLevelShiftMusicChainCardObejct(levelID, ____difficultySingle, ____difficultyDouble, isUtage, ____utageQuestionMarkSingleDigit, ____utageQuestionMarkDoubleDigit);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ResultMonitor), "SetLevel")]
    private static void FixLevelShiftTotalResultPlayer(MusicLevelID levelID, ref SpriteCounter ____difficultySingle, ref SpriteCounter ____difficultyDouble)
    {
        FixLevelShiftMusicChainCardObejct(levelID, ____difficultySingle, ____difficultyDouble, false, null, null);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TrackStartMonitor), "SetTrackStart")]
    private static void FixLevelShiftTrackStartMonitor(
        int ___monitorIndex,
        ref SpriteCounter ____difficultySingle,
        ref SpriteCounter ____difficultyDouble,
        GameObject ____utageQuestionSingleDigit,
        GameObject ____utageQuestionDoubleDigit)
    {
        var music = Singleton<DataManager>.Instance.GetMusic(GameManager.SelectMusicID[___monitorIndex]);
        var levelID = (MusicLevelID)music.notesData[GameManager.SelectDifficultyID[___monitorIndex]].musicLevelID;
        FixLevelShiftMusicChainCardObejct(levelID, ____difficultySingle, ____difficultyDouble, music.name.id >= 100000, ____utageQuestionSingleDigit, ____utageQuestionDoubleDigit);
    }
}