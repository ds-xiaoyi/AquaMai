using System.Diagnostics;
using AquaMai.Config.Attributes;
using AquaMai.Core.Resources;
using HarmonyLib;
using Main;
using UnityEngine;

namespace AquaMai.Mods.UX;

[ConfigSection(
    defaultOn: true,
    zh: "因 AmDaemon 未运行而黑屏时显示警告")]
public class NoAmDaemonAlert : MonoBehaviour
{
    private static NoAmDaemonAlert display;
    private Stopwatch stopwatch = new Stopwatch();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMainObject), "Awake")]
    public static void OnGameMainObjectAwake()
    {
        var go = new GameObject("黑屏提示组件");
        display = go.AddComponent<NoAmDaemonAlert>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Main.GameMain), "LateInitialize", typeof(MonoBehaviour), typeof(Transform), typeof(Transform))]
    public static void LateInitialize(MonoBehaviour gameMainObject)
    {
        if (display == null) return;
        Destroy(display);
    }

    private void Start()
    {
        stopwatch.Start();
    }

    private void OnGUI()
    {
        if (stopwatch.ElapsedMilliseconds < 2000)
        {
            return;
        }
        GUIStyle styleTitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 35,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };

        GUI.Label(new Rect(50, 50, Screen.width - 50, Screen.height - 500), Locale.NoAmDaemonAlertTitle, styleTitle);
        GUI.Label(new Rect(50, 50, Screen.width - 50, Screen.height - 50), Locale.NoAmDaemonAlertMessage, style);
    }
}