using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using GameRandom = Pigeon.Math.Random;

/// <summary>
/// Fixes Heavy (and any other OverrideThenAdd fire-interval upgrade) showing
/// different fire-rate roll brackets depending on equip/stack context.
///
/// Vanilla path:
///   GetAdditiveData(appliedCount) → Override when count==0, Add when count>0
///   CreateInverse converts those differently against live fireInterval
///   → e.g. [-33.33% - -21.05%] vs [-60% - -55.88%] for Heavy on a 0.3s gun.
///
/// Fix: always display using first-copy Override semantics and the gear prefab's
/// baseline fireInterval. Combat Apply() is left unchanged.
/// </summary>
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class HeavyBugfixPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.heavybugfix";
    public const string PluginName = "HeavyBugfix";
    public const string PluginVersion = "1.0.0";

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

[HarmonyPatch(typeof(UpgradeProperty_FireInterval), nameof(UpgradeProperty_FireInterval.GetStatData))]
internal static class FireIntervalGetStatDataPatch
{
    /// <summary>
    /// Replace vanilla GetStatData. Always use appliedCount 0 so OverrideThenAdd
    /// upgrades (Heavy) keep a stable Override-relative bracket, and resolve the
    /// prefab fire interval so live modified stats cannot shift the range.
    /// </summary>
    private static bool Prefix(
        UpgradeProperty_FireInterval __instance,
        GameRandom rand,
        IUpgradable gear,
        UpgradeInstance upgrade,
        ref IEnumerator<StatData> __result)
    {
        __result = BuildStatData(__instance, rand, gear, upgrade);
        return false;
    }

    private static IEnumerator<StatData> BuildStatData(
        UpgradeProperty_FireInterval instance,
        GameRandom rand,
        IUpgradable gear,
        UpgradeInstance upgrade)
    {
        // appliedCount 0 → OverrideThenAdd stays Override for display.
        OverrideData<Range<float>> displayData = instance.fireInterval.GetAdditiveData(0);
        float baseline = GetPrefabFireInterval(gear);
        yield return StatData.CreateInverse(
            TextBlocks.GetString("firerate"),
            displayData,
            upgrade,
            ref rand,
            baseline,
            BoostParams.MinIsBetter);
    }

    private static float GetPrefabFireInterval(IUpgradable gear)
    {
        IUpgradable prefab = gear;
        try
        {
            if (gear != null)
            {
                IUpgradable resolved = gear.GetPrefab();
                if (resolved != null)
                    prefab = resolved;
            }
        }
        catch (Exception ex)
        {
            HeavyBugfixPlugin.Log?.LogDebug($"GetPrefab failed, using provided gear: {ex.Message}");
        }

        if (prefab is IWeapon prefabWeapon)
            return prefabWeapon.GunData.fireInterval;

        if (gear is IWeapon liveWeapon)
            return liveWeapon.GunData.fireInterval;

        // Safe fallback: CreateInverse with p=1 leaves Override values unchanged.
        return 1f;
    }
}
