using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;
using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventoring
{
    [HarmonyPatch]
    public class InventoryPatches
    {
        [HarmonyPrefix]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPatch(typeof(Inventory), "GetItem", typeof(string))]
        public static bool GetItemReversed(Inventory __instance, string name, List<ItemDrop.ItemData> ___m_inventory,
            ref ItemDrop.ItemData __result)
        {
            if (!Plugin.Settings.TakeItemsInReverseOrder.Value) return true;

            for (var index = ___m_inventory.Count - 1; index >= 0; index--)
            {
                var itemData = ___m_inventory[index];
                if (itemData.m_shared.m_name != name) continue;
                __result = itemData;
                return false;
            }

            __result = null;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPatch(typeof(Inventory), "CountItems", typeof(string))]
        private static void CountItemsPatch(Inventory __instance, string name, ref int __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
                return;
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            var playerItemCount = __instance.CountItemsOriginal(name);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containerItemCount = containers
                .Sum(container => container.Container.GetInventory().CountItemsOriginal(name));
            __result = playerItemCount + containerItemCount;
        }

        [HarmonyPrefix]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
        public static bool RemoveItemPatch(Inventory __instance, string name, int amount,
            List<ItemDrop.ItemData> ___m_inventory)
        {
            Plugin.Log.LogDebug($"Trying to remove item from {__instance.GetHashCode()}");
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
            {
                Plugin.Log.LogDebug($"Not tracked {__instance.GetHashCode()}");
                return true;
            }
            Plugin.Log.LogDebug($"player: {player.GetPlayerName()} ({player.GetInstanceID()} via {__instance.GetHashCode()})");
            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player,
                Plugin.Settings.ContainerLookupRange.Value);
            Plugin.Log.LogDebug($"RemoveItem got {containers.Count} containers");
            InventoryItemRemover.IterateAndRemoveItemsFromInventories(player, containers, name, amount,
                out var removalReport);

            if (Plugin.Settings.AddExtractionEffectWhenCrafting.Value)
                foreach (var removal in removalReport.Removals
                        .Where(removal => removal.TrackedContainer.HasValue))
                    // ReSharper disable once PossibleInvalidOperationException
                    InventoryItemRemover.SpawnEffect(player, removal.TrackedContainer.Value);

            Plugin.Log.LogDebug(removalReport.GetReportString());
            if (Plugin.Settings.LogItemRemovalsToConsole.Value)
                Console.instance.Print($"{removalReport.GetReportString(true)}");
            return false;
        }

        [HarmonyPostfix]
        [HarmonyBefore("randyknapp.mods.equipmentandquickslots")]
        [HarmonyPatch(typeof(Inventory), "HaveItem", typeof(string))]
        public static void HaveItemPatch(Inventory __instance, string name, ref bool __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value
                || !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player)
                || __result)
                return;

            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containersHaveAny = containers
                .Any(container => container.Container.GetInventory().HaveItemOriginal(name));
            Plugin.Log.LogDebug(
                $"Player ${player.GetPlayerID()} found {containersHaveAny} of {name} via {containers.Count} containers");
            __result = containersHaveAny;
        }
    }
}