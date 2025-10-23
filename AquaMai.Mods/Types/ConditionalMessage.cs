using System.Linq;
using AquaMai.Core.Helpers;
using JetBrains.Annotations;

namespace AquaMai.Mods.Types;

public abstract class ConditionalMessage
{
    public string[] locales = [];
    [CanBeNull] public string minimumAquaMaiVersion = null;
    [CanBeNull] public string maximumAquaMaiVersion = null;
    public int minimumGameVersion = 0;
    public int maximumGameVersion = 0;

    public bool ShouldShow()
    {
        if (locales != null && locales.Length != 0 && !locales.Contains(General.locale))
        {
            return false;
        }

        var aquaMaiVersion = new System.Version(Core.BuildInfo.Version);
        if (minimumAquaMaiVersion != null && aquaMaiVersion < new System.Version(minimumAquaMaiVersion))
        {
            return false;
        }

        if (maximumAquaMaiVersion != null && aquaMaiVersion > new System.Version(maximumAquaMaiVersion))
        {
            return false;
        }

        var gameVersion = GameInfo.GameVersion;
        if (minimumGameVersion != 0 && gameVersion < minimumGameVersion)
        {
            return false;
        }

        if (maximumGameVersion != 0 && gameVersion > maximumGameVersion)
        {
            return false;
        }

        return true;
    }
}