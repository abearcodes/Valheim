using System.Linq;
using ABearCodes.Valheim.CraftingWithContainers.Tracking;
using JetBrains.Annotations;

namespace ABearCodes.Valheim.CraftingWithContainers.Crafting
{
    public static class InventoryDetours
    {
        [UsedImplicitly]
        public static int CountItemsArea(this Inventory inventory, string itemName)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player)) 
                return inventory.CountItems(itemName);
            // only run our handler if we are tracking the inventory, hence it's a 
            // player inventory. otherwise - let the default execute as is
            var playerItemCount = inventory.CountItems(itemName);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containerItemCount = containers
                .Sum(container => container.Container.GetInventory().CountItems(itemName));
            return playerItemCount + containerItemCount;
        }
        
        [UsedImplicitly]
        public static bool HaveItemArea(this Inventory inventory, string itemName)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player)) 
                return inventory.HaveItem(itemName);
            
            var playerHasAny = player.GetInventory().HaveItem(itemName);
            var containers = ContainerTracker
                .GetViableContainersInRangeForPlayer(player, Plugin.Settings.ContainerLookupRange.Value);
            var containersHaveAny = containers
                .Any(container => container.Container.GetInventory().HaveItem(itemName));
            Plugin.Log.LogDebug(
                $"Player ${player.GetPlayerID()} found {playerHasAny}+{containersHaveAny} of {itemName} via {containers.Count}");
            return playerHasAny || containersHaveAny;
        }

        [UsedImplicitly]
        public static void RemoveItemArea(this Inventory inventory, string name, int amount)
        {
            if (!Plugin.Settings.CraftingWithContainersEnabled.Value ||
                !ContainerTracker.PlayerByInventoryDict.TryGetValue(inventory.GetHashCode(), out var player))
            {
                inventory.RemoveItem(name, amount);
                return;
            }
            var containers = ContainerTracker.GetViableContainersInRangeForPlayer(player,
                Plugin.Settings.ContainerLookupRange.Value);
            InventoryItemRemover.IterateAndRemoveItemsFromInventories(player, containers, name, amount, out var removalReport);

            if (Plugin.Settings.AddExtractionEffectWhenCrafting.Value)
            {
                foreach (var removal in removalReport.Removals
                    .Where(removal => removal.TrackedContainer.HasValue))
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    InventoryItemRemover.SpawnEffect(player, removal.TrackedContainer.Value);
                }
            }
            
            Plugin.Log.LogDebug(removalReport.GetReportString());
            if (Plugin.Settings.LogItemRemovalsToConsole.Value)
            {
                Console.instance.Print($"{removalReport.GetReportString(true)}");
            }
        }
    }
}