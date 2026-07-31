using System;
using System.Collections.Generic;
using HarmonyLib;
using GameRandom = Pigeon.Math.Random;

[HarmonyPatch(typeof(UpgradeProperty_FireInterval), nameof(UpgradeProperty_FireInterval.GetStatData))]
internal static class FireIntervalGetStatDataPatch
{
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
        var displayData = instance.fireInterval.GetAdditiveData(0);
        var baseline = GetPrefabFireInterval(gear);
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
        var prefab = gear;
        try
        {
            if (gear != null)
            {
                var resolved = gear.GetPrefab();
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

        return 1f;
    }
}