using System;
using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Patches;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    [HarmonyPatch]
    public static class ItemFindingPatches2
    {
        // [HarmonyPatch(typeof(Inventory), "CountItems")]
        // [HarmonyPostfix]
        static void CountItems(Inventory __instance, string name, ref int __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            if (!ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player)
            )
                return;
            
            Plugin.Log.LogDebug($"{__instance.GetHashCode()} ({player.GetPlayerName()}) - {name}");
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containersCount = containers.Select(tracked => tracked.Container.GetInventory())
                .Sum(inventory => inventory.CountItems(name));
            Plugin.Log.LogDebug($"Search for item {name} for {player.GetPlayerName()}: player - {__result}, containers - {containersCount}");
            // __result already contains player item count
            __result += containersCount;
        }
        
        // [HarmonyPatch(typeof(Inventory), "HaveItem")]
        // [HarmonyPostfix]
        static void HaveItem(Inventory __instance, string name, ref bool __result)
        {
            if (__result) return;
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            if (!ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player)
            )
                return;
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            __result = containers.Select(tracked => tracked.Container.GetInventory())
                .Any(inventory => inventory.HaveItem(name));
        }
        
        
        [HarmonyPatch(typeof(Inventory), "GetAllItems", typeof(string), typeof(List<ItemDrop.ItemData>))]
        [HarmonyPostfix]
        private static void GetAllItems(Inventory __instance, string name, List<ItemDrop.ItemData> items)
        {
            Plugin.Log.LogDebug($"YO {__instance.GetHashCode()} - {name}");
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            if (!ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
                return;
            Plugin.Log.LogDebug($"{__instance.GetHashCode()} ({player.GetPlayerName()}) - {name}");
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var allItemsFromAllContainers = containers.Select(tracked => tracked.Container.GetInventory())
                .SelectMany(inventory => inventory.GetAllItems())
                .ToList();
            Plugin.Log.LogDebug($"{player.GetPlayerName()} added {allItemsFromAllContainers.Count} to temp items");
            items.AddRange(allItemsFromAllContainers);
        }
    }
}