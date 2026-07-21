using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class CompareUpgradesPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.compareupgrades";
    public const string PluginName = "CompareUpgrades";
    public const string PluginVersion = "2.0.0";

    internal static ManualLogSource Log;
    internal static CompareUpgradesPlugin Instance;

    private ConfigEntry<Key> compareKey;
    private Harmony harmony;

    internal static Key CompareKey => Instance != null ? Instance.compareKey.Value : Key.C;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        compareKey = Config.Bind(
            "Keybinds",
            "Compare Key",
            Key.C,
            "Hotkey to lock/unlock an upgrade for side-by-side comparison while the gear details window is open.");

        try
        {
            harmony = new Harmony(PluginGUID);
            harmony.PatchAll(typeof(ComparePatches));
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to initialize: {ex}");
        }
    }

    private void Update()
    {
        try
        {
            CompareController.Tick();
        }
        catch (Exception ex)
        {
            Log.LogError($"CompareController tick failed: {ex}");
        }
    }

    private void OnDestroy()
    {
        try
        {
            CompareController.Shutdown();
        }
        catch (Exception ex)
        {
            Log.LogError($"Shutdown failed: {ex}");
        }

        try
        {
            harmony?.UnpatchSelf();
        }
        catch (Exception ex)
        {
            Log.LogError($"Unpatch failed: {ex}");
        }

        Instance = null;
    }
}
