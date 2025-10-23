using AquaMai.Config.Attributes;
using AquaMai.Core;
using DB;
using HarmonyLib;

namespace AquaMai.Mods.UX;

[ConfigSection(defaultOn: true, exampleHidden: true)]
public static class TestModeHook
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TestmodeRootTableRecord), MethodType.Constructor, typeof(int), typeof(string), typeof(string), typeof(string))]
    public static void ShowAquaMaiVersion(int EnumValue, ref string ___Name)
    {
        if (EnumValue != 0) return;
        ___Name += $"\n<size=90%>AquaMai {BuildInfo.GitVersion}";
    }
}