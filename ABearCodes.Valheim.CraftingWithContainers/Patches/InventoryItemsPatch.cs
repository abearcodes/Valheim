using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(string), typeof(int))]
    public class RemoveItemsFromConnectedInventoriesWhenCraftingPatch
    {
        // ReSharper disable once UnusedMember.Global
        public static bool Prefix(ref Inventory __instance, string name, int amount)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value) return true;
            if (!InventoryTracker.ExpandedPlayerInventories
                .TryGetValue(__instance.GetHashCode(), out var linkedInventories))
                return true;

            
            Plugin.Log.LogDebug($"Received {linkedInventories.Containers.Count() + 1} to remove {amount} of {name}");
            IterateAndRemoveItemsFromInventories(linkedInventories, name, amount);
            Plugin.Log.LogDebug($"Removed {amount} of {name}");
            return false;
        }

        private static void IterateAndRemoveItemsFromInventories(InventoryTracker.LinkedInventories linkedInventories,
            string name, int amount)
        {
            var leftToRemove = amount;
            if (Plugin.Settings.TakeFromPlayerInventoryFirst.Value)
            {
                Plugin.Log.LogDebug($"Taking from player first for order {name} ({amount})");
                leftToRemove = RemoveFromInventory(linkedInventories.Player.GetInventory(), name, amount);
                Plugin.Log.LogDebug($"Left to remove {leftToRemove}");
            }
            foreach (var container in linkedInventories.Containers)
            {
                leftToRemove = RemoveFromInventory(container.GetInventory(), name, leftToRemove);
                UpdateContainerNetworkData(linkedInventories.Player, container);
                if (leftToRemove == 0)
                    break;
            }
            if (!Plugin.Settings.TakeFromPlayerInventoryFirst.Value && leftToRemove > 0)
            {
                RemoveFromInventory(linkedInventories.Player.GetInventory(), name, amount);
            }
        }

        public static void UpdateContainerNetworkData(Player player, Container container)
        {
            var containerZNewView = (ZNetView) (typeof(Container).GetField("m_nview", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(container));
            ReversePatches.ContainerSave(container);
            var containerUid = containerZNewView.GetZDO().m_uid;
            ZDOMan.instance.ForceSendZDO(player.GetPlayerID(), containerUid);
            containerZNewView.GetZDO().SetOwner(player.GetPlayerID());
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