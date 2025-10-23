using System;
using System.Collections.Generic;
using System.Linq;
using AquaMai.Config.Interfaces;

namespace AquaMai.Config.Migration;

public class ConfigMigrationManager : IConfigMigrationManager
{
    public static readonly ConfigMigrationManager Instance = new();

    private readonly Dictionary<string, IConfigMigration> migrationMap =
        new List<IConfigMigration>
        {
            new ConfigMigration_V1_0_V2_0(),
            new ConfigMigration_V2_0_V2_1(),
            new ConfigMigration_V2_1_V2_2(),
            new ConfigMigration_V2_2_V2_3(),
        }.ToDictionary(m => m.FromVersion);

    public string LatestVersion { get; }

    private ConfigMigrationManager()
    {
        LatestVersion = migrationMap.Values
            .Select(m => m.ToVersion)
            .OrderByDescending(version =>
            {
                var versionParts = version.Split('.').Select(int.Parse).ToArray();
                return versionParts[0] * 100000 + versionParts[1];
            })
            .First();
    }

    public IConfigView Migrate(IConfigView config)
    {
        var currentVersion = GetVersion(config);
        while (migrationMap.ContainsKey(currentVersion))
        {
            var migration = migrationMap[currentVersion];
            Utility.Log($"Migrating config from v{migration.FromVersion} to v{migration.ToVersion}");
            config = migration.Migrate((ConfigView)config);
            currentVersion = migration.ToVersion;
        }

        if (currentVersion != LatestVersion)
        {
            throw new ArgumentException($"Could not migrate the config from v{currentVersion} to v{LatestVersion}");
        }

        return config;
    }

    public string GetVersion(IConfigView config)
    {
        if (config.TryGetValue<string>("Version", out var version))
        {
            return version;
        }

        // Assume v1.0 if not found
        return "1.0";
    }
}