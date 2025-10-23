﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using AquaMai.Config.Attributes;
using AquaMai.Core.Helpers;
using HarmonyLib;
using Main;
using MelonLoader;
using UnityEngine;
using BuildInfo = AquaMai.Core.BuildInfo;

namespace AquaMai.Mods.Utils;

[ConfigSection(
    name: "崩溃报错",
    en: "Show error log in the game.",
    zh: "在游戏中显示错误日志窗口而不是关闭游戏进程",
    defaultOn: true)]
public class ShowErrorLog
{
    private static Ui _errorUi;

    [ConfigEntry(
        name: "崩溃窗口",
        en: "Use new error handler",
        zh: "使用新版错误报告生成器")]
    private static readonly bool useNewErrorHandler = true;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMain), "ExceptionHandler")]
    private static void ExceptionHandler(GameMain __instance, Exception e)
    {
        if (_errorUi == null)
        {
            _errorUi = new GameObject("ErrorUI").AddComponent<Ui>();
            _errorUi.gameObject.SetActive(true);
        }

        string logFile = $"{MAI2System.Path.ErrorLogPath}{DateTime.Now:yyyyMMddHHmmss}.log";
        MelonLogger.Msg("Error Log:");
        if (File.Exists(logFile))
        {
            MelonLogger.Error(File.ReadAllText(logFile));
            _errorUi.SetErrorLog(File.ReadAllText(logFile));
        }
        else
        {
            MelonLogger.Error(e);
            _errorUi.SetErrorLog(e.ToString());
        }

        if (useNewErrorHandler)
        {
            Application.quitting += ApplicationOnQuittingNew;
        }
        else
        {
            Application.quitting += ApplicationOnQuitting;
            _errorUi.StartCoroutine(_errorUi.Show());
        }
    }

    private static void ApplicationOnQuitting()
    {
        Thread.Sleep(Timeout.Infinite);
    }

    public static void ApplicationOnQuittingNew()
    {
        var path = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            using var s = GetErrorReporterStream();
            s.CopyTo(fs);
        }

        MelonLogger.Msg("Starting Crash Handler...");
        var psi = new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = Path.GetDirectoryName(Application.dataPath),
            UseShellExecute = false,
        };
        System.Diagnostics.Process.Start(psi);
    }

    private static Stream GetErrorReporterStream()
    {
        var s = BuildInfo.ModAssembly.Assembly.GetManifestResourceStream("AquaMai.ErrorReport.exe");
        if (s != null)
        {
            return s;
        }

        s = BuildInfo.ModAssembly.Assembly.GetManifestResourceStream("AquaMai.ErrorReport.exe.compressed");
        return new DeflateStream(s, CompressionMode.Decompress);
    }

    private class Ui : MonoBehaviour
    {
        private string _errorLog = "";

        public void SetErrorLog(string text)
        {
            _errorLog = "Error Log:\n" + text;
        }

        public void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = GuiSizes.FontSize,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState() { textColor = Color.black }
            };

            var boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = new GUIStyleState() { background = Texture2D.whiteTexture }
            };

            int logLineCount = Regex.Matches(_errorLog, "\n").Count + 1;
            float offset = GuiSizes.PlayerCenter * 0.12f;
            var x = GuiSizes.PlayerCenter / 2f + offset / 2f;
            var y = Screen.height / 1.8f;
            var width = GuiSizes.PlayerCenter - offset;
            var height = GuiSizes.LabelHeight * logLineCount + GuiSizes.Margin * 2;

            GUI.Box(new Rect(x, y, width, height), "", boxStyle);
            GUI.Label(new Rect(x, y, width, height), _errorLog, labelStyle);
            if (!GuiSizes.SinglePlayer)
            {
                GUI.Box(new Rect(x + GuiSizes.PlayerWidth, y, width, height), "", boxStyle);
                GUI.Label(new Rect(x + GuiSizes.PlayerWidth, y, width, height), _errorLog, labelStyle);
            }
        }

        public IEnumerator Show()
        {
            while (true)
            {
                yield return null; // 让 Unity 处理一帧
            }
        }
    }
}