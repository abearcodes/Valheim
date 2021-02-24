using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    [HarmonyPatch]
    public static class ItemFindingPatches
    {
        [HarmonyPatch(typeof(Inventory), "CountItems")]
        [HarmonyPostfix]
        static void CountItems(Inventory __instance, string name, ref int __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            var player = ContainerTrackingExtensions.PlayerByInventoryDict[__instance.GetHashCode()];

            // Plugin.Log.LogDebug($"{__instance.GetHashCode()} ({player.GetPlayerName()} - {playerID}) - {name}");
            var containers = ContainerTrackingExtensions
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            // Plugin.Log.LogDebug($"{containers.Count()}");
            var playerCount = ReversePatches.InventoryCountItems(__instance, name);
            var containersCount = containers.Select(tracked => tracked.Container.GetInventory())
                .Sum(inventory => ReversePatches.InventoryCountItems(inventory, name));
            // Plugin.Log.LogDebug($"Search for item {name} for {player.GetPlayerName()}({playerID}): player - {playerCount}, containers - {containersCount}");
            __result = playerCount + containersCount;
        }


        /*
         *  [HarmonyPatch(typeof(Inventory), "CountItems")]
public class CountConnectedInventoriesWhenCraftingPatch
{
    // ReSharper disable once UnusedMember.Global
    public static void Postfix(ref Inventory __instance, string name, ref int __result)
    {
        if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
        if (!InventoryTracker.ExpandedPlayerInventories.TryGetValue(__instance.GetHashCode(),
            out var extraInventories))
            return;
        var sum = new[] {extraInventories.Player.GetInventory()}
            .Concat(extraInventories.Containers.Select(container => container.GetInventory()))
            .Sum(inventory => ReversePatches.InventoryCountItems(inventory, name));
        __result = sum;
    }
}
         */
    }
}