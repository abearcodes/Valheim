using System.Collections.Generic;
using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch(typeof(Inventory), "CountItems")]
    public class CountConnectedInventoriesWhenCraftingPatch
    {
        // ReSharper disable once UnusedMember.Global
        public static void Postfix(ref Inventory __instance, string name, ref int __result)
        {
            if (!Valheim.CraftingWithContainers.Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            if (!InventoryTracker.ExpandedPlayerInventories.TryGetValue(__instance.GetHashCode(),
                out var extraInventories))
                return;
            var sum = new[] {extraInventories.Player.GetInventory()}
                .Concat(extraInventories.Containers.Select(container => container.GetInventory()))
                .Sum(inventory => ReversePatches.InventoryCountItems(inventory, name));
            __result = sum;
        }
    }

    [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
    public class RemoveItemsFromConnectedInventoriesWhenCraftingPatch
    {
        // ReSharper disable once UnusedMember.Global
        public static bool Prefix(ref Inventory __instance, string name, int amount)
        {
            if (!Valheim.CraftingWithContainers.Plugin.Settings.CraftingWithContainersEnabled.Value) return true;
            if (!InventoryTracker.ExpandedPlayerInventories
                .TryGetValue(__instance.GetHashCode(), out var extraInventories))
                return true;

            var allAccessibleInventories = MergeInventories(extraInventories);
            Valheim.CraftingWithContainers.Plugin.Log.LogDebug($"Received {allAccessibleInventories.Count} to remove {amount} of {name}");
            IterateAndRemoveItemsFromInventories(name, amount, allAccessibleInventories);

            Valheim.CraftingWithContainers.Plugin.Log.LogDebug($"Removed {amount} of {name}");
            return false;
        }

        private static List<Inventory> MergeInventories(InventoryTracker.LinkedInventories extraInventories)
        {
            var allAccessibleInventories = extraInventories.Containers
                .Select(container => container.GetInventory())
                .ToList();
            if (Valheim.CraftingWithContainers.Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
                allAccessibleInventories.Insert(0, extraInventories.Player.GetInventory());
            else
                allAccessibleInventories.Add(extraInventories.Player.GetInventory());
            return allAccessibleInventories;
        }

        private static void IterateAndRemoveItemsFromInventories(string name, int amount,
            List<Inventory> allAccessibleInventories)
        {
            var leftToRemove = amount;
            foreach (var inventory in allAccessibleInventories)
            {
                var currentInventoryCount = ReversePatches.InventoryCountItems(inventory, name);
                var itemsToTake = currentInventoryCount <= leftToRemove
                    ? currentInventoryCount % leftToRemove
                    : leftToRemove;
                ReversePatches.InventoryRemoveItemByString(inventory, name, itemsToTake);
                leftToRemove -= itemsToTake;
                Valheim.CraftingWithContainers.Plugin.Log.LogDebug($"Removed {itemsToTake} of {amount} from inv {inventory.GetHashCode()}");

                if (leftToRemove == 0)
                    break;
            }
        }
    }
}