
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace ABearCodes.Valheim.CraftingWithContainers.Patches
{
    [HarmonyPatch(typeof(Inventory), "CountItems")]
    public class CountConnectedInventoriesWhenCraftingPatch
    {
        // ReSharper disable once UnusedMember.Global
        public static void Postfix(Inventory __instance, string name, ref int __result)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return;
            var craftingLock = Tracking.Tracker.FindLockByPlayer(__instance);
            if (craftingLock == null) return;

            var sum = new[] {craftingLock.Player.GetInventory()}
                .Concat(craftingLock.Entries.Select(entry => entry.Container.GetInventory()))
                .Where(entry => entry != null)
                .Sum(inventory => ReversePatches.InventoryCountItems(inventory, name));
            __result = sum;
        }
    }

    [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
    public class RemoveItemsFromConnectedInventoriesWhenCraftingPatch
    {
        // ReSharper disable once UnusedMember.Global
        public static bool Prefix(Inventory __instance, string name, int amount)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return true;
            var craftingLock = Tracking.Tracker.CraftingPlayers.Find(entry =>
                entry.Player.GetInventory().GetHashCode() == __instance.GetHashCode());
            if (craftingLock == null) return true;
            Plugin.Log.LogDebug($"Received {craftingLock.Entries.Count() + 1} to remove {amount} of {name}");
            IterateAndRemoveItemsFromInventories(craftingLock, name, amount);
            Plugin.Log.LogDebug($"Removed {amount} of {name}");
            return false;
        }

        private static void IterateAndRemoveItemsFromInventories(Tracking.Tracker.CraftingLock craftingLock,
            string name, int amount)
        {
            var leftToRemove = amount;
            if (Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                Plugin.Log.LogDebug($"Taking from player first for order {name} ({amount})");
                leftToRemove = RemoveFromInventory(craftingLock.Player.GetInventory(), name, amount);
                Plugin.Log.LogDebug($"Left to remove {leftToRemove}");
            }
            foreach (var container in craftingLock.Entries.Select(entry => entry.Container))
            {
                leftToRemove = RemoveFromInventory(container.GetInventory(), name, leftToRemove);
                if (leftToRemove == 0)
                    break;
            }
            if (!Plugin.Settings.TakeFromPlayerInventoryFirst.Value && leftToRemove > 0)
            {
                RemoveFromInventory(craftingLock.Player.GetInventory(), name, amount);
            }
        }
        
        private static int RemoveFromInventory(Inventory inventory, string name, int leftToRemove)
        {
            var currentInventoryCount = ReversePatches.InventoryCountItems(inventory, name);
            var itemsToTake = currentInventoryCount < leftToRemove
                ? currentInventoryCount
                : leftToRemove;
            ReversePatches.InventoryRemoveItemByString(inventory, name, itemsToTake);
            leftToRemove -= itemsToTake;
            Plugin.Log.LogDebug($"Removed {itemsToTake} from inv {inventory.GetHashCode()}");
            return leftToRemove;
        }
    }
}