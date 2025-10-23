﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AquaMai.Config.Attributes;
using UnityEngine;

namespace AquaMai.Mods.GameSystem;

[ConfigSection(
    name: "图形设置",
    en: "Resolution Settings / Windowed Mode.",
    zh: "分辨率设置 / 窗口化")]
public class Window
{
    [ConfigEntry(
        name: "窗口模式",
        en: "Window the game.",
        zh: "窗口化游戏")]
    private static readonly bool windowed = false;

    [ConfigEntry(
        name: "无边框",
        en: "Borderless window.")]
    private static readonly bool borderless = false;

    [ConfigEntry(
        name: "宽度",
        en: """
            Window width (and height) for windowed mode, rendering resolution for fullscreen mode.
            If set to 0, windowed mode will remember the user-set size, fullscreen mode will use the current display resolution.
            If the display size is incorrect, please set it to the screen resolution.
            """,
        zh: """
            宽度（和高度）窗口化时为游戏窗口大小，全屏时为渲染分辨率
            如果设为 0，窗口化将记住用户设定的大小，全屏时将使用当前显示器分辨率
            全屏时如果显示大小不正确，请改成屏幕的分辨率（如 2160x3840）
            """)]
    private static readonly int width = 0;

    [ConfigEntry(
        name: "高度",
        en: "Height, as above.")]
    private static readonly int height = 0;

    private const int GWL_STYLE = -16;
    private const uint WS_WHATEVER = 0x14CF0000;
    private const uint WS_VISIBLE = 0x10000000;    
    private const uint WS_POPUP = 0x80000000;

    private static IntPtr hwnd = IntPtr.Zero;

    public static void OnBeforePatch()
    {
        if (windowed)
        {
            var alreadyWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            if (width == 0 || height == 0)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
            else
            {
                alreadyWindowed = false;
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
            }

            hwnd = GetWindowHandle();
            if (alreadyWindowed)
            {
                SetResizeable();
            }
            else
            {
                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    // Screen.SetResolution has delay
                    SetResizeable();
                });
            }
        }
        else
        {
            var width = Window.width == 0 ? Display.main.systemWidth : Window.width;
            var height = Window.height == 0 ? Display.main.systemHeight : Window.height;
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }
    }

    public static void SetResizeable()
    {
        if (hwnd == IntPtr.Zero) return;
        if (borderless)
        {
            SetWindowLongPtr(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        }
        else
        {
            SetWindowLongPtr(hwnd, GWL_STYLE, WS_WHATEVER);
        }
    }

    private delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

    [DllImport("Kernel32.dll")]
    static extern int GetCurrentThreadId();

    static IntPtr GetWindowHandle()
    {
        IntPtr returnHwnd = IntPtr.Zero;
        var threadId = GetCurrentThreadId();
        EnumThreadWindows(threadId,
            (hWnd, lParam) =>
            {
                if (returnHwnd == IntPtr.Zero) returnHwnd = hWnd;
                return true;
            }, IntPtr.Zero);
        return returnHwnd;
    }

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, uint dwNewLong);
}
