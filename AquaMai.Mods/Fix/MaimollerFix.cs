using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AquaMai.Config.Attributes;
using AquaMai.Core.Attributes;
using AquaMai.Mods.GameSystem;
using DB;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using KeyCodeID = AquaMai.Config.Types.KeyCodeID;

namespace AquaMai.Mods.Fix;

[ConfigSection(exampleHidden: true, defaultOn: true)]
[EnableIf(nameof(shouldEnable))]
public class MaimollerFix
{
    private static readonly Assembly shit = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(it => it.GetName().Name == "ADXHIDIOMod");
    private static bool shouldEnable = shit != null;

    public static void OnBeforePatch()
    {
        AddMap(KeyMap.Test, KeyCode.ScrollLock);
        AddMap(KeyMap.Service, KeyCode.Pause);
        AddMap(KeyMap.Button1_1P, KeyCode.W);
        AddMap(KeyMap.Button2_1P, KeyCode.E);
        AddMap(KeyMap.Button3_1P, KeyCode.D);
        AddMap(KeyMap.Button4_1P, KeyCode.C);
        AddMap(KeyMap.Button5_1P, KeyCode.X);
        AddMap(KeyMap.Button6_1P, KeyCode.Z);
        AddMap(KeyMap.Button7_1P, KeyCode.A);
        AddMap(KeyMap.Button8_1P, KeyCode.Q);
        AddMap(KeyMap.Select_1P, KeyCode.Alpha3);
        AddMap(KeyMap.Button1_2P, KeyCode.Keypad8);
        AddMap(KeyMap.Button2_2P, KeyCode.Keypad9);
        AddMap(KeyMap.Button3_2P, KeyCode.Keypad6);
        AddMap(KeyMap.Button4_2P, KeyCode.Keypad3);
        AddMap(KeyMap.Button5_2P, KeyCode.Keypad2);
        AddMap(KeyMap.Button6_2P, KeyCode.Keypad1);
        AddMap(KeyMap.Button7_2P, KeyCode.Keypad4);
        AddMap(KeyMap.Button8_2P, KeyCode.Keypad7);
        AddMap(KeyMap.Select_2P, KeyCode.KeypadMultiply);
#if DEBUG
        MelonLogger.Msg("[MaimollerFix] KeyCode map initialized.");
        foreach (var keyCode in shitMap)
        {
            MelonLogger.Msg("[MaimollerFix] Key code: " + keyCode);
        }
#endif
    }

    private static Dictionary<KeyCode, KeyCode> shitMap = [];

    private static void AddMap(KeyCodeID origin, KeyCode to)
    {
        shitMap[(KeyCode)((DB.KeyCodeID)origin).GetValue()] = to;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ADXHIDIO.ADXController.IOIO", "GetKey")]
    public static bool GetShit(KeyCode keyCode, ref bool __result, Dictionary<KeyCode, bool> ___state)
    {
        if (!shitMap.TryGetValue(keyCode, out KeyCode result)) return true;
        __result = ___state[result];
        return false;
    }

}