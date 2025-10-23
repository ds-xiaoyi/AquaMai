namespace AquaMai.Core.Types;

/// <summary>
/// 加入选歌的设置界面的设置项
/// </summary>
public interface IPlayerSettingsItem
{
    public int Sort { get; }
    public string Name { get; }
    public string Detail { get; }
    public string SpriteFile { get; }

    public bool IsLeftButtonActive { get; }
    public bool IsRightButtonActive { get; }
    
    /// <summary>
    /// 用于显示的“当前值”
    /// </summary>
    public string GetOptionValue(int player);

    /// <summary>
    /// 按下增加时调用
    /// 由于 patch 了 GetOptionValueIndex 和 GetOptionMax，点击屏幕的事件永远会被响应，所以这里面需要自己判断区间
    /// </summary>
    public void AddOption(int player);

    /// <summary>
    /// 按下减少时调用
    /// </summary>
    public void SubOption(int player);
}
