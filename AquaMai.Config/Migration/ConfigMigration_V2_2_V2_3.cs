using AquaMai.Config.Interfaces;
using Tomlet.Models;

namespace AquaMai.Config.Migration;

public class ConfigMigration_V2_2_V2_3 : IConfigMigration
{
    public string FromVersion => "2.2";
    public string ToVersion => "2.3";

    public ConfigView Migrate(ConfigView src)
    {
        var dst = (ConfigView)src.Clone();
        dst.SetValue("Version", ToVersion);

        if (src.TryGetValue<float>("GameSystem.SinglePlayer.Radius", out var Radius))
        {
            dst.SetValue("GameSystem.ExteraMouseInput.Radius", Radius);
            dst.Remove("GameSystem.SinglePlayer.Radius");
        }

        if (src.TryGetValue<bool>("GameSystem.SinglePlayer.DisplayArea", out var DisplayArea))
        {
            dst.SetValue("GameSystem.ExteraMouseInput.DisplayArea", DisplayArea);
            dst.Remove("GameSystem.SinglePlayer.DisplayArea");
        }

        return dst;
    }
}