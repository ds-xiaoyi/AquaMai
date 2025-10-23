using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace AquaMai;

public static class AssemblyLoader
{
    public enum AssemblyName
    {
        ConfigInterfaces,
        Config,
        Core,
        Mods,
    }

    private static readonly Dictionary<AssemblyName, string> Assemblies = new()
    {
        [AssemblyName.ConfigInterfaces] = "AquaMai.Config.Interfaces.dll",
        [AssemblyName.Config] = "AquaMai.Config.dll",
        [AssemblyName.Core] = "AquaMai.Core.dll",
        [AssemblyName.Mods] = "AquaMai.Mods.dll",
    };

    private static readonly Dictionary<AssemblyName, Assembly> LoadedAssemblies = [];

    public static Assembly GetAssembly(AssemblyName assemblyName) => LoadedAssemblies[assemblyName];

    public static void LoadAssemblies()
    {
        foreach (var (assemblyName, assemblyFileName) in Assemblies)
        {
# if DEBUG
            MelonLoader.MelonLogger.Msg($"Loading assembly \"{assemblyFileName}\"...");
# endif
            LoadedAssemblies[assemblyName] = LoadAssemblyFromResource(assemblyFileName);
        }
    }

    public static Assembly LoadAssemblyFromResource(string assemblyName)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        using var decompressedStream = executingAssembly.GetManifestResourceStream(assemblyName);
        if (decompressedStream != null)
        {
            return AppDomain.CurrentDomain.Load(StreamToBytes(decompressedStream));
        }
        using var compressedStream = executingAssembly.GetManifestResourceStream($"{assemblyName}.compressed");
        if (compressedStream != null)
        {
            return AppDomain.CurrentDomain.Load(DecompressToBytes(compressedStream));
        }
        throw new Exception($"Embedded assembly \"{assemblyName}\" not found.");
    }

    private static byte[] StreamToBytes(Stream stream)
    {
        if (stream == null)
        {
            return [];
        }
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static byte[] DecompressToBytes(Stream stream) => StreamToBytes(new DeflateStream(stream, CompressionMode.Decompress));
}
