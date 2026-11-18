using System;
using System.IO;
using Godot;

namespace STS2MobileIos.Patches;

// The 4x global time scale from jhaizhou-ops/sts2-ios has been removed: the game runs at normal speed
// (use the game's own Fast Mode setting if wanted). Only the opt-in memory check is kept here.
public static class TimeScalePatch
{
    // postfix on MegaCrit.Sts2.Core.Nodes.NGame._Ready
    public static void ReadyPostfix()
    {
        try
        {
            // Memory headroom check (off by default): put a file named memcheck_on in the app's Documents
            // (Files app) and each launch appends a line to memcheck.txt. os_proc_available_memory is how much
            // more memory iOS lets this process use: ~7 GB+ with the increased-memory-limit entitlement, ~3 GB without.
            var docs = OS.GetUserDataDir();
            if (File.Exists(Path.Combine(docs, "memcheck_on")))
            {
                var line = $"{DateTime.Now:MM-dd HH:mm:ss} available memory {OsProcAvailableMemory() / 1048576} MB";
                PatchHelper.Log($"[MemCheck] {line}");
                File.AppendAllText(Path.Combine(docs, "memcheck.txt"), line + "\n");
            }
        }
        catch (Exception e)
        {
            PatchHelper.Log($"[MemCheck] failed: {e.Message}");
        }
    }

    [System.Runtime.InteropServices.DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "os_proc_available_memory")]
    private static extern ulong OsProcAvailableMemory();
}
