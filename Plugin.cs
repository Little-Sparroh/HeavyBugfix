using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class HeavyBugfixPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.heavybugfix";
    public const string PluginName = "HeavyBugfix";
    public const string PluginVersion = "1.0.1";

    internal static ManualLogSource Log;

    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;
        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll(typeof(FireIntervalGetStatDataPatch));
        Log.LogInfo($"{PluginName} v{PluginVersion} loaded — fire-interval range display stabilized.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}