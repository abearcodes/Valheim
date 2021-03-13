using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Tracking
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerLifecyclePatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake(Player __instance)
        {
            Plugin.Log.LogDebug(
                $"Tracking player {__instance.GetPlayerID()} - {__instance.GetInventory().GetHashCode()}");
            ContainerTracker.PlayerByInventoryDict[__instance.GetInventory().GetHashCode()] = __instance;
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        private static void OnDestroy(Player __instance)
        {
            Plugin.Log.LogDebug(
                $"Removing player {__instance.GetPlayerID()} - {__instance.GetInventory().GetHashCode()}");
            ContainerTracker.PlayerByInventoryDict[__instance.GetInventory().GetHashCode()] = __instance;
        }
    }
}