using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Common;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Inventory
{
    [HarmonyPatch]
    public class InventoryPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Inventory), "GetItem", typeof(string))]
        public static bool GetItemReversed(global::Inventory __instance, string name, List<ItemDrop.ItemData> ___m_inventory,
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
        [HarmonyPatch(typeof(global::Inventory), "CountItems", typeof(string))]
        private static void CountItemsPatch(global::Inventory __instance, string name, ref int __result)
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
        [HarmonyPatch(typeof(global::Inventory), "RemoveItem", typeof(string), typeof(int))]
        public static bool RemoveItemPatch(global::Inventory __instance, string name, int amount,
            List<ItemDrop.ItemData> ___m_inventory)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(__instance.GetHashCode(), out var player))
            {
                __instance.RemoveItemOriginal(name, amount);
                return true;
            }

            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player,
                Plugin.Settings.ContainerLookupRange.Value);

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
        [HarmonyPatch(typeof(global::Inventory), "HaveItem", typeof(string))]
        public static void HaveItemPatch(global::Inventory __instance, string name, ref bool __result)
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