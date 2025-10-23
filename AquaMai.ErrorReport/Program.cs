using System.Runtime.InteropServices;

namespace AquaMai.ErrorReport;

public class Program
{
    [DllImport("SHCore.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetProcessDpiAwareness(DPI_AWARENESS awareness);

    // 定义 DPI_AWARENESS 枚举
    public enum DPI_AWARENESS
    {
        DPI_AWARENESS_INVALID = -1,
        DPI_UNAWARE = 0,
        SYSTEM_AWARE = 1,
        PER_MONITOR_AWARE = 2
    }

    [STAThread]
    public static void Main()
    {
        SetProcessDpiAwareness(DPI_AWARENESS.SYSTEM_AWARE);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new CrashForm());
    }
}