using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CompareUpgrades;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class CompareUpgradesPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.compareupgrades";
    public const string PluginName = "CompareUpgrades";
    public const string PluginVersion = "2.0.1";

    internal static ManualLogSource Log;
    internal static CompareUpgradesPlugin Instance;

    private Harmony harmony;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        ConfigManager.Initialize(Config, Log);

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
            ConfigManager.Tick();
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
            ConfigManager.Dispose();
        }
        catch (Exception ex)
        {
            Log.LogError($"Config dispose failed: {ex}");
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